using LendingSolution.Application.Exceptions;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LendingSolution.API.Controllers;

[Authorize]
[ApiController]
[Route("api/company")]
public class CompanyController(
    ICompanyService companySErvice,
    ILoanProductService loanProductService,
    ISupportService supportService,
    ILoanService loanService,
    ILogger<CompanyController> logger)
  : Controller
{
    private readonly ILoanProductService _loanProductService = loanProductService;
    private readonly ICompanyService _companyService = companySErvice;
    private readonly ISupportService _supportService = supportService;
    private readonly ILoanService _loanService = loanService;
    private readonly ILogger<CompanyController> _logger = logger;

    [Authorize(Roles = "Admin")]
    [HttpPost("product/create")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateLoanProductRequestDto body)
    {
        try
        {
            // Get company ID from JWT
            var companyIdClaim = User.FindFirstValue("CompanyId");
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                return BadRequest(ApiResponse.Fail("Company ID not found in token"));
            }

            var result = await _loanProductService.CreateLoanProduct(body, companyId);
            _logger.LogInformation("Loan product created successfully for company {CompanyId}", companyId);
            return Ok(ApiResponse.Ok("Loan product created successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating loan product.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [Authorize(Roles = "SuperAdmin, Admin")]
    [HttpGet("loan-products/{companyId}")]
    public async Task<IActionResult> GetLoanProductsByCmopanyId([FromRoute] Guid companyId)
    {
        try
        {
            var result = await _loanProductService.GetLoanProductsByCompanyId(companyId);
            _logger.LogInformation("Loan products fetched successfully for company {CompanyId}", companyId);
            return Ok(ApiResponse.Ok("Loan products fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching loan products.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("sa/create")]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequestDto body)
    {
        try
        {
            var result = await _companyService.CreateCompany(body);
            _logger.LogInformation("Company created successfully");
            return Ok(ApiResponse.Ok("Company created successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating company.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("sa/deactivate/{companyId}")]
    public async Task<IActionResult> DeactivateCompany([FromRoute] Guid companyId)
    {
        try
        {
            await _companyService.Deactivate(companyId);
            _logger.LogInformation("Company {CompanyId} deactivated successfully", companyId);
            return Ok(ApiResponse.Ok("Company deactivated successfully", new { }));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deactivating company.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("sa/activate/{companyId}")]
    public async Task<IActionResult> Activate([FromRoute] Guid companyId)
    {
        try
        {
            await _companyService.Activate(companyId);
            _logger.LogInformation("Company {CompanyId} activated successfully", companyId);
            return Ok(ApiResponse.Ok("Company activated successfully", new { }));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while activating company.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // Admin can update their own company info
    [Authorize(Roles = "Admin")]
    [HttpPut("info")]
    public async Task<IActionResult> UpdateCompanyInfo([FromBody] UpdateCompanyRequestDto body)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse.Fail("User not authenticated"));
            }

            // Get user's company
            var userCompanyResult = await _companyService.GetUserCompany(userId);

            _logger.LogInformation("Company info updated successfully by user {UserId}", userId);
            return Ok(userCompanyResult);
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating company info.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // Admin can update loan product info for their company
    [Authorize(Roles = "Admin")]
    [HttpPut("loan-product/{productId}")]
    public async Task<IActionResult> UpdateLoanProduct(Guid productId, [FromBody] UpdateLoanProductRequestDto body)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse.Fail("User not authenticated"));
            }

            var result = await _loanProductService.UpdateLoanProduct(productId, body, userId);

            _logger.LogInformation("Loan product {ProductId} updated successfully by user {UserId}", productId, userId);
            return Ok(ApiResponse.Ok("Loan product updated successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating loan product.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetCompanyDashboard()
    {
        try
        {
            // Get the company ID from the user's claims or context
            var companyId = User.FindFirstValue("CompanyId");
            if (string.IsNullOrEmpty(companyId))
            {
                return BadRequest(ApiResponse.Fail("Company ID not found in user context"));
            }

            var result = await _supportService.GetCompanyDashboardAsync(companyId);

            _logger.LogInformation("Company dashboard fetched successfully for company {CompanyId}", companyId);
            return Ok(ApiResponse.Ok("Company dashboard fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching company dashboard.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets loan products for the admin's company with filtering and search
    /// </summary>
    /// <param name="filter">Filter parameters for searching and filtering loan products</param>
    /// <returns>Paginated list of loan products for the admin's company</returns>
    // [GET] /api/company/loan-products
    [HttpGet("loan-products")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetCompanyLoanProducts([FromQuery] LoanProductFilterDto filter)
    {
        try
        {
            // Get company ID from JWT
            var companyIdClaim = User.FindFirstValue("CompanyId");
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                return BadRequest(ApiResponse.Fail("Company ID not found in token"));
            }

            var result = await _loanProductService.GetCompanyLoanProductsAsync(companyId, filter);
            _logger.LogInformation("Company loan products fetched successfully for company {CompanyId}", companyId);
            return Ok(ApiResponse.Ok("Company loan products fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching company loan products.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets all loans for the admin's company with filtering and search
    /// </summary>
    /// <param name="filter">Filter parameters for searching and filtering loans</param>
    /// <returns>Paginated list of loans for the admin's company</returns>
    // [GET] /api/company/loans[
    [HttpGet("loans")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetCompanyLoans([FromQuery] LoanFilterDto filter)
    {
        try
        {
            // Get company ID from JWT
            var companyIdClaim = User.FindFirstValue("CompanyId");
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                return BadRequest(ApiResponse.Fail("Company ID not found in token"));
            }

            var result = await _loanService.GetCompanyLoansAsync(companyId, filter);
            _logger.LogInformation("Company loans fetched successfully for company {CompanyId}", companyId);
            return Ok(ApiResponse.Ok("Company loans fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching company loans.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets a specific loan by ID (Admin - must belong to their company)
    /// </summary>
    /// <param name="loanId">Loan ID</param>
    /// <returns>Loan details if it belongs to the admin's company</returns>
    // [GET] /api/company/loans/{loanId}
    [HttpGet("loans/{loanId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetLoanById(Guid loanId)
    {
        try
        {
            // Get company ID from JWT
            var companyIdClaim = User.FindFirstValue("CompanyId");
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                return BadRequest(ApiResponse.Fail("Company ID not found in token"));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _loanService.GetLoanByIdAsync(loanId, userId);

            _logger.LogInformation("Loan {LoanId} fetched successfully", loanId);
            return Ok(ApiResponse.Ok("Loan fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching loan by ID.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets all companies for SuperAdmin with advanced filtering and search
    /// </summary>
    /// <param name="filter">Filter parameters for searching and filtering companies</param>
    /// <returns>Paginated list of all companies with analytics data</returns>
    // [GET] /api/company/sa/all
    [HttpGet("sa/all")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetAllCompanies([FromQuery] CompanyFilterDto filter)
    {
        try
        {
            var result = await _companyService.GetAllCompaniesAsync(filter);

            _logger.LogInformation("All companies fetched successfully with filters");
            return Ok(ApiResponse.Ok("All companies fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching all companies.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets a specific company by ID for SuperAdmin
    /// </summary>
    /// <param name="companyId">The ID of the company to retrieve</param>
    /// <returns>Company details with analytics</returns>
    // [GET] /api/company/sa/{companyId}
    [HttpGet("sa/{companyId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetCompanyById(Guid companyId)
    {
        try
        {
            var result = await _companyService.GetCompanyByIdAsync(companyId);

            _logger.LogInformation("Company {CompanyId} fetched successfully", companyId);
            return Ok(ApiResponse.Ok("Company fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching company by ID.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets the current admin's company information (no companyId required)
    /// </summary>
    /// <returns>Company details for the authenticated admin</returns>
    // [GET] /api/company/my-company
    [HttpGet("my-company")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetMyCompany()
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("GetMyCompany called for user {UserId}", userId);

            // Debug: Log all claims in the token
            var allClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
            _logger.LogInformation("User claims: {@Claims}", allClaims);

            // Get company ID from JWT
            var companyIdClaim = User.FindFirstValue("CompanyId");

            if (string.IsNullOrEmpty(companyIdClaim))
            {
                _logger.LogWarning("CompanyId claim not found in JWT for user {UserId}. Available claims: {@Claims}", userId, allClaims);
                return BadRequest(ApiResponse.Fail("Company ID not found in token. Please ensure you are logged in as an Admin user associated with a company."));
            }

            if (!Guid.TryParse(companyIdClaim, out var companyId))
            {
                _logger.LogWarning("Invalid CompanyId format in JWT: {CompanyIdClaim} for user {UserId}", companyIdClaim, userId);
                return BadRequest(ApiResponse.Fail("Invalid company ID format in token"));
            }

            _logger.LogInformation("Retrieved CompanyId {CompanyId} from JWT for user {UserId}", companyId, userId);

            var result = await _companyService.GetCompanyByIdAsync(companyId);

            _logger.LogInformation("Admin's company {CompanyId} fetched successfully for user {UserId}", companyId, userId);
            return Ok(ApiResponse.Ok("Admin's company fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching admin's company.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // Admin can update their own company logo
    [Authorize(Roles = "Admin")]
    [HttpPatch("logo")]
    public async Task<IActionResult> UpdateCompanyLogo([FromBody] UpdateCompanyLogoDto body)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse.Fail("User not authenticated"));
            }

            // Get company ID from JWT
            var companyIdClaim = User.FindFirstValue("CompanyId");
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                return BadRequest(ApiResponse.Fail("Company ID not found in token"));
            }

            var company = await _companyService.GetCompanyById(companyId);
            
            // Create an update request with current company data but new logo
            var updateRequest = new UpdateCompanyRequestDto
            {
                Name = company.Name,
                ShortName = company.ShortName,
                Street = company.Street,
                City = company.City,
                State = company.State,
                LogoDocumentId = body.LogoDocumentId
            };

            var result = await _companyService.UpdateCompany(companyId, updateRequest, userId);

            _logger.LogInformation("Company logo updated successfully by user {UserId} for company {CompanyId}", userId, companyId);
            return Ok(ApiResponse.Ok("Company logo updated successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating company logo.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    // SuperAdmin can update any company logo
    [Authorize(Roles = "SuperAdmin")]
    [HttpPatch("sa/{companyId}/logo")]
    public async Task<IActionResult> UpdateCompanyLogoByAdmin(Guid companyId, [FromBody] UpdateCompanyLogoDto body)
    {
        try
        {
            var company = await _companyService.GetCompanyById(companyId);
            
            // Create an update request with current company data but new logo
            var updateRequest = new UpdateCompanyRequestDto
            {
                Name = company.Name,
                ShortName = company.ShortName,
                Street = company.Street,
                City = company.City,
                State = company.State,
                LogoDocumentId = body.LogoDocumentId
            };

            var result = await _companyService.UpdateCompany(companyId, updateRequest);

            _logger.LogInformation("Company {CompanyId} logo updated successfully by SuperAdmin", companyId);
            return Ok(ApiResponse.Ok("Company logo updated successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating company logo.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets all users that belong to the admin's company
    /// </summary>
    /// <param name="filter">Filter parameters for searching and filtering users</param>
    /// <returns>Paginated list of company users with statistics</returns>
    // [GET] /api/company/users
    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetCompanyUsers([FromQuery] CompanyUserFilterDto filter)
    {
        try
        {
            // Get company ID from JWT
            var companyIdClaim = User.FindFirstValue("CompanyId");
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                return BadRequest(ApiResponse.Fail("Company ID not found in token"));
            }

            var result = await _companyService.GetCompanyUsersAsync(companyId, filter);
            _logger.LogInformation("Company users fetched successfully for company {CompanyId}", companyId);
            return Ok(ApiResponse.Ok("Company users fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching company users.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

    /// <summary>
    /// Gets all users that belong to a specific company (SuperAdmin only)
    /// </summary>
    /// <param name="companyId">The ID of the company</param>
    /// <param name="filter">Filter parameters for searching and filtering users</param>
    /// <returns>Paginated list of company users with statistics</returns>
    // [GET] /api/company/sa/{companyId}/users
    [HttpGet("sa/{companyId}/users")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> GetCompanyUsersByCompanyId(Guid companyId, [FromQuery] CompanyUserFilterDto filter)
    {
        try
        {
            var result = await _companyService.GetCompanyUsersAsync(companyId, filter);
            _logger.LogInformation("Company users fetched successfully for company {CompanyId}", companyId);
            return Ok(ApiResponse.Ok("Company users fetched successfully", result));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, ex.Message);
            return StatusCode(ex.StatusCode, ApiResponse.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while fetching company users.");
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

}
