using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LendingSolution.Core.Models;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Infrastructure.Data;
using LendingSolution.Core.Enum;
using LendingSolution.Application.Repositories.Interfaces;
namespace LendingSolution.Application.Services.Implementations;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    ApplicationDbContext db,
    ICompanyRepository companyRepository,
    ILoanRepository loanRepository,
    IRemitaService remitaService
) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ITokenService _tokenService = tokenService;
    private readonly ApplicationDbContext _db = db;
    private readonly ICompanyRepository _companyRepository = companyRepository;
    private readonly ILoanRepository _loanRepository = loanRepository;
    private readonly IRemitaService _remitaService = remitaService;

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

        return ApiResponse.Ok("Personal details saved successfully");
    }

    public async Task<ApiResponse> CreateSuperAdmin(CreateSuperAdminRequestDto body)
    {
        var user = new ApplicationUser
        {
            FirstName = body.FirstName,
            LastName = body.LastName,
            Email = body.Email,
            UserName = body.Email
        };

        var result = await _userManager.CreateAsync(user, body.Password);

        if (!result.Succeeded)
        {
            return ApiResponse.Fail("Failed to create super admin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // Assign SuperAdmin role
        await _userManager.AddToRoleAsync(user, "SuperAdmin");

        return ApiResponse.Ok("Super admin created successfully", new { userId = user.Id });
    }
}