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
public class CompanyController(ICompanyService companySErvice, ILoanProductService loanProductService, ISupportService supportService, ILogger<CompanyController> logger)
  : Controller
{
    private readonly ILoanProductService _loanProductService = loanProductService;
    private readonly ICompanyService _companyService = companySErvice;
    private readonly ISupportService _supportService = supportService;
    private readonly ILogger<CompanyController> _logger = logger;

    [Authorize(Roles = "SuperAdmin, Admin")]
    [HttpPost("product/create")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateLoanProductRequestDto body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
                ApiResponse.Fail("Invalid model State")
            );
        }

        var result = await _loanProductService.CreateLoanProduct(body);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin, Admin")]
    [HttpGet("loan-products/{companyId}")]
    public async Task<IActionResult> GetLoanProductsByCmopanyId([FromRoute] Guid companyId)
    {
        try
        {
            var result = await _loanProductService.GetLoanProductsByCompanyId(companyId);
            return Ok(ApiResponse.Ok("Loan products fetched successfully", result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching loan products for company {CompanyId}", companyId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred: " + ex.Message));
        }

    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpPost("sa/create")]
    public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequestDto body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
               ApiResponse.Fail("Invalid model state")
           );
        }

        var result = await _companyService.CreateCompany(body);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("sa/deactivate/{companyId}")]
    public async Task<IActionResult> DeactivateCompany([FromRoute] Guid companyId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
                ApiResponse.Fail("Invalid model state")
            );
        }

        var result = await _companyService.Deactivate(companyId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [Authorize(Roles = "SuperAdmin")]
    [HttpGet("sa/activate/{companyId}")]
    public async Task<IActionResult> Activate([FromRoute] Guid companyId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
                ApiResponse.Fail("Invalid model state")
            );
        }

        var result = await _companyService.Activate(companyId);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    // Admin can update their own company info
    [Authorize(Roles = "Admin")]
    [HttpPut("info")]
    public async Task<IActionResult> UpdateCompanyInfo([FromBody] UpdateCompanyRequestDto body)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail("Invalid model state"));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse.Fail("User not authenticated"));
            }

            // Get user's company
            var userCompanyResult = await _companyService.GetUserCompany(userId);
            if (!userCompanyResult.Success)
            {
                return BadRequest(userCompanyResult);
            }

            var companyData = userCompanyResult.Data as CompanyResponseDto;
            if (companyData == null)
            {
                return BadRequest(ApiResponse.Fail("Unable to retrieve company information"));
            }

            // Update the company
            var result = await _companyService.UpdateCompany(companyData.Id, body, userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating company info for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse.Fail("Invalid model state"));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(ApiResponse.Fail("User not authenticated"));
            }

            // Verify the loan product belongs to the user's company
            var productResult = await _loanProductService.GetLoanProductById(productId);
            if (!productResult.Success)
            {
                return BadRequest(productResult);
            }

            var productData = productResult.Data as LoanProductResponseDto;
            if (productData == null)
            {
                return BadRequest(ApiResponse.Fail("Unable to retrieve loan product information"));
            }

            // Get user's company to verify ownership
            var userCompanyResult = await _companyService.GetUserCompany(userId);
            if (!userCompanyResult.Success)
            {
                return BadRequest(userCompanyResult);
            }

            var companyData = userCompanyResult.Data as CompanyResponseDto;
            if (companyData == null || companyData.Id != productData.CompanyId)
            {
                return Forbid(ApiResponse.Fail("You can only update loan products for your own company").Message);
            }

            // Update the loan product
            var result = await _loanProductService.UpdateLoanProduct(productId, body, userId);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating loan product {ProductId} for user {UserId}", productId, User.FindFirstValue(ClaimTypes.NameIdentifier));
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
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving company dashboard for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred"));
        }
    }

}
