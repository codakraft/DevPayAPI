using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using LendingSolution.Core.Models;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Infrastructure.Data;
using LendingSolution.Core.Enum;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Exceptions;
namespace LendingSolution.Application.Services.Implementations;

public class AuthService(
    UserManager<ApplicationUser> userManager,
    ITokenService tokenService,
    ApplicationDbContext db,
    ICompanyRepository companyRepository,
    ILoanRepository loanRepository,
    IRemitaService remitaService,
    RoleManager<IdentityRole> roleManager
) : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly ITokenService _tokenService = tokenService;
    private readonly ApplicationDbContext _db = db;
    private readonly ICompanyRepository _companyRepository = companyRepository;
    private readonly ILoanRepository _loanRepository = loanRepository;
    private readonly IRemitaService _remitaService = remitaService;
    private readonly RoleManager<IdentityRole> _roleManager = roleManager;

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
        ApplicationUser? user;

        user = await _userManager.FindByEmailAsync(body.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, body.Password))
        {
            return ApiResponse.Fail("Invalid credentials");
        }

        var token = await _tokenService.GenerateTokenAsync(user);

        var data = new
        {
            Token = token,
            user = new
            {
                Id = user?.Id,
                FirstName = user?.FirstName ?? string.Empty,
                LastName = user?.LastName ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                PhoneNumber = user?.PhoneNumber ?? string.Empty
            }
        };

        return ApiResponse.Ok("Logged in successfully", data);
    }

    public async Task<object> AdminLogin(LoginRequestDto body)
    {
        var user = await _userManager.FindByEmailAsync(body.Email);

        if (user is null || !await _userManager.CheckPasswordAsync(user, body.Password))
        {
            throw new AppException("Invalid credentials");
        }

        var token = await _tokenService.GenerateTokenAsync(user);

        var data = new
        {
            Token = token,
            user = new
            {
                Id = user?.Id,
                FirstName = user?.FirstName ?? string.Empty,
                LastName = user?.LastName ?? string.Empty,
                Email = user?.Email ?? string.Empty,
                PhoneNumber = user?.PhoneNumber ?? string.Empty
            }
        };

        return data;
    }

    public bool VerifyOtp(VerifyOtpRequestDto body)
    {
        if (body.Otp != "1234")
        {
            throw new AppException("Invalid OTP");
        }
        return true;
    }

    public async Task<bool> SavePersonalDetails(SavePersonalDetailsRequestDto body, System.Security.Claims.ClaimsPrincipal user)
    {
        if (body is null)
        {
            throw new AppException("Request body cannot be null");
        }

        var userId = _userManager.GetUserId(user);

        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new AppException("User not found");
        }

        var loan = await _db.Loans.FirstOrDefaultAsync(l => l.Id == body.LoanId && l.UserId == userId);

        if (loan == null)
        {
            throw new AppException("Loan not found for user");
        }

        var existingEmployee = await _db.Employees.FirstOrDefaultAsync(e => e.UserId == userId && e.LoanId == body.LoanId);

        if (existingEmployee != null)
        {
            throw new AppException("Personal details already submitted for this loan");
        }

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

        return true;
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

    public async Task<ApiResponse> CreateAdmin(CreateAdminRequestDto body)
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
            return ApiResponse.Fail("Failed to create admin: " + string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        // Assign SuperAdmin role
        await _userManager.AddToRoleAsync(user, "Admin");

        return ApiResponse.Ok("admin created successfully", new { userId = user.Id });
    }

    public Task<ApiResponse> GetRoles()
    {
        var roles = _roleManager.Roles.Select(r => new
        {
            r.Id,
            r.Name
        })
        .ToList();

        return Task.FromResult(ApiResponse.Ok("Roles fetched successfully", roles));
    }

    public async Task<ApiResponse> AssignRole(RoleAssignDto body)
    {
        var user = await _userManager.FindByIdAsync(body.UserId);

        if (user is null)
        {
            return ApiResponse.Fail("User not found");
        }

        var role = await _roleManager.FindByIdAsync(body.RoleId);

        if (role is null)
        {
            return ApiResponse.Fail("Role not found.");
        }

        var result = await _userManager.AddToRoleAsync(user, role.Name!);

        return ApiResponse.Ok("role added successfully", result);
    }
}