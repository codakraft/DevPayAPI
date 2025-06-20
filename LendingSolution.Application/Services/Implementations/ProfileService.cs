using System.Security.Claims;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Models;
using LendingSolution.Core.Models.Response;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace LendingSolution.Application.Services.Implementations;

public class ProfileService(UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor) : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IHttpContextAccessor _contextAccessor = contextAccessor;
    public async Task<ApiResponse> GetUserProfile()
    {
        var httpContext = _contextAccessor.HttpContext;
        Console.WriteLine(httpContext);
        if (httpContext is null || httpContext.User is null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "Could not fetch user profile",
                Data = httpContext
            };
        }

        var email = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
        if (email == null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "Could not fetch user profile",
                Data = null
            };
        }

        var user = await _userManager.FindByEmailAsync(email);

        if (user is null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "Could not fetch user profile - chee",
                Data = null
            };
        }

        var user_response = new {
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            user.Address,
            user.City,
            user.State,
            user.DateOfBirth,
        };

        return new ApiResponse
        {
            Success = true,
            Data = user_response,
            Message = "User fetched successfully"
        };
    }

}