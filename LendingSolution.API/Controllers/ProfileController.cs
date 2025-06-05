using LendingSolution.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController(IProfileService profileService) : Controller
{
    private readonly IProfileService _profileService = profileService;

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _profileService.GetUserProfile();
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}
