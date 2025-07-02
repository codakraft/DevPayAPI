using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.API.Controllers;

[Authorize]
[ApiController]
[Route("api/company")]
public class CompanyController(ICompanyService companySErvice, ILoanProductService loanProductService, ILogger<CompanyController> logger)
  : Controller
{
    private readonly ILoanProductService _loanProductService = loanProductService;
    private readonly ICompanyService _companyService = companySErvice;
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

    // Update company information

    // [GET]    /api/company  
    // [GET]    /api/company/{id}  
    // [POST]   /api/company  
    // [PUT]    /api/company/{id}  
    // [GET]    /api/company/products  
    // [POST]   /api/company/products  
    // [PUT]    /api/company/products/{id}  
    // [DELETE] /api/company/products/{id}  
    // [GET]    /api/company/analytics  


}
