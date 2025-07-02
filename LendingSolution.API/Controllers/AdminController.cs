using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController(IAuthService authService, ICompanyService companySErvice) : Controller
{
    private readonly IAuthService _authService = authService;
    private readonly ICompanyService _companyService = companySErvice;

    // [POST]   /api/admin/users/{id}/assign-role
    [HttpPost("role/assign")]
    public async Task<IActionResult> AssignRole([FromBody] RoleAssignDto body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(
                ApiResponse.Fail("Invalid model state")
            );
        }

        var result = await _authService.AssignRole(body);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    // [POST]   /api/admin/roles
    // [GET]    /api/admin/settings  
    // [PUT]    /api/admin/settings  
    // [GET]    /api/admin/approvals/pending  
    // [POST]   /api/admin/approvals/{id}/approve
    // [POST]   /api/admin/approvals/{id}/reject


}

