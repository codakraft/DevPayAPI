using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LendingSolution.Core.Models;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Infrastructure.Data;
using LendingSolution.Core.Enum;
namespace LendingSolution.Application.Services.Implementations;

public class AuthService(UserManager<ApplicationUser> userManager, ITokenService tokenService, ApplicationDbContext db) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ITokenService _tokenService = tokenService;
    private readonly ApplicationDbContext _db = db;

    public async Task<ApiResponse> Register(RegisterRequestDto body)
    {
        var existingCompany = await _db.Companies
            .FirstOrDefaultAsync(c => c.Id == body.CompanyId);
        if (existingCompany == null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "Company not found",
                Data = null
            };
        }

        var existingEmail = await _userManager.Users.FirstOrDefaultAsync(u => u.Email == body.Email);

        if (existingEmail != null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "A user with this email already exists.",
                Data = null
            };
        }

        var existingBvn = await _db.Account.FirstOrDefaultAsync(u => u.Bvn == body.Bvn);

        if (existingBvn != null)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "Bvn already exists.",
                Data = null
            };
        }

        // Add new user
        var user = new ApplicationUser
        {
            UserName = body.Email,
            Email = body.Email,
            FirstName = string.Empty,
            LastName = string.Empty,
            Address = string.Empty,
            City = string.Empty,
            State = string.Empty,
            DateOfBirth = body.DateOfBirth
        };

        var result = await _userManager.CreateAsync(user);

        if (!result.Succeeded)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "User creation failed",
                Data = result.Errors
            };
        }

        // add a new loan
        var loan = new Loan
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Amount = 0,
            DurationInMonths = 0,
            Purpose = string.Empty,
            Status = LoanStatus.NotBooked,
            CompanyId = body.CompanyId
        };

        _db.Loans.Add(loan);

        var loanResult = await _db.SaveChangesAsync();

        if (loanResult <= 0)
        {
            return new ApiResponse
            {
                Success = false,
                Message = "Loan creation failed",
                Data = null
            };
        }

        return new ApiResponse
        {
            Success = true,
            Data = loan.Id,
            Message = "User created successfully"
        };
    }

    private bool VerifyBvn(String Bvn)
    {
        // if (!Bvn.Contains("2222222"))
        // {
        //     return false;
        // }
        return true;
    }

    public async Task<ApiResponse> Login(LoginRequestDto body)
    {
        var result = new ApiResponse { };
        ApplicationUser? user;

        user = await _userManager.FindByEmailAsync(body.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, body.Password))
        {
            result.Success = false;
            result.Message = "Invalid credentials";
            result.Data = null;

            return result;
        }

        var token = _tokenService.GenerateTokenAsync(user);

        result.Success = true;
        result.Data = new
        {
            Token = token,
            user = new
            {
                FirstName = user?.FirstName ?? string.Empty,
                LastName = user?.LastName ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                PhoneNumber = user?.PhoneNumber ?? string.Empty
            }
        };

        return result;
    }

    public async Task<ApiResponse> AdminLogin(LoginRequestDto body)
    {
        var result = new ApiResponse { };

        var user = await _userManager.FindByEmailAsync(body.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, body.Password))
        {
            result.Success = false;
            result.Message = "Invalid credentials";
            result.Data = null;

            return result;
        }

        var token = _tokenService.GenerateTokenAsync(user);

        result.Success = true;
        result.Data = new
        {
            Token = token,
            user = new
            {
                FirstName = user?.FirstName ?? string.Empty,
                LastName = user?.LastName ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                PhoneNumber = user?.PhoneNumber ?? string.Empty
            }
        };

        return result;
    }

    public ApiResponse VerifyOtp(VerifyOtpRequestDto body)
    {
        if (body.Otp == "1234")
        {
            return new ApiResponse
            {
                Data = null,
                Success = true,
                Message = "OTP verified successfully"
            };
        }
        else
        {
            return new ApiResponse
            {
                Data = null,
                Success = false,
                Message = "Invalid OTP"
            };
        }
    }

    public ApiResponse SalaryHistoryReview(ReviewHistoryRequestDto body)
    {
        // Example logic, replace with real implementation as needed
        var response = new ReviewHistoryResponseDto
        {
            CompanyName = "Example Company",
            MaxEligibleAmount = 500000.00m
        };
        return new ApiResponse
        {
            Data = response,
            Success = true,
            Message = "Salary history reviewed successfully"
        };
    }

    public async Task<ApiResponse> SavePersonalDetails(SavePersonalDetailsRequestDto body, System.Security.Claims.ClaimsPrincipal user)
    {
        // Validate input
        if (body == null)
        {
            return new ApiResponse { Success = false, Message = "Request body cannot be null", Data = null };
        }

        // Check for valid user
        var userId = _userManager.GetUserId(user);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new ApiResponse { Success = false, Message = "User not found", Data = null };
        }

        // Check if Loan exists and belongs to user
        var loan = await _db.Loans.FirstOrDefaultAsync(l => l.Id == body.LoanId && l.UserId == userId);
        if (loan == null)
        {
            return new ApiResponse { Success = false, Message = "Loan not found for user", Data = null };
        }

        var existingEmployee = await _db.Employees.FirstOrDefaultAsync(e => e.UserId == userId && e.LoanId == body.LoanId);
        if (existingEmployee != null)
        {
            return new ApiResponse { Success = false, Message = "Personal details already submitted for this loan", Data = null };
        }

        // Save employee details
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Employer = body.Employer,
            Industry = body.Industry,
            Role = body.Role,
            ResidentialAddress = body.ResidentialAddress,
            LoanId = body.LoanId
        };
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        return new ApiResponse { Success = true, Message = "Personal details saved successfully", Data = null };
    }
}