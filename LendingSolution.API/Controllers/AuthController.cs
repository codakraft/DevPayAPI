using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Models.Response;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : Controller
{
    private readonly IAuthService _authService = authService;

    [HttpPost("onboarding")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Data = null,
                Success = false,
                Message = "Invalid model State"
            });
        }

        var result = await _authService.Register(body);


        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Data = null,
                Success = false,
                Message = "Invalid model State"
            });
        }

        var result = await _authService.Login(body);

        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPost("verify-otp")]
    public IActionResult VerifyOtp([FromBody] VerifyOtpRequestDto body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Data = null,
                Success = false,
                Message = "Invalid model State"
            });
        }

        var result = _authService.VerifyOtp(body);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("save-personal-details")]
    public async Task<IActionResult> SavePersonalDetails([FromBody] SavePersonalDetailsRequestDto body)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new ApiResponse
            {
                Data = null,
                Success = false,
                Message = "Invalid model State"
            });
        }

        var result = await _authService.SavePersonalDetails(body, User);
        if (result.Success)
            return Ok(result);
        return BadRequest(result);
    }
}
