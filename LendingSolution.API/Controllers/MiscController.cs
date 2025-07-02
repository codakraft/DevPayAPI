using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.API.Controllers;

[ApiController]
[Route("api/misc")]
public class MiscController(IAuthService authService) : Controller
{
    private readonly IAuthService _authService = authService;

    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await _authService.GetRoles();
        return Ok(ApiResponse.Ok("roles fetched successfully", roles));
    }
}