using LendingSolution.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.Controllers;

[ApiController]
[Route("api/admin/loans")]
[Authorize(Roles = "Admin")]
public class AdminLoanController : Controller
{
    private readonly ILoanService _loanService;

    public AdminLoanController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpPost("{loanId}/approve")]
    public async Task<IActionResult> ApproveLoan(Guid loanId)
    {
        var result = await _loanService.ApproveLoan(loanId);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllLoans()
    {
        var result = await _loanService.GetAllLoans();
        return Ok(result);
    }
}

// After creating the user
// Send email (pseudo-code, use your email service)
// await _emailSender.SendEmailAsync(user.Email, "Registration Successful", "Welcome to LendingSolution!");

// Or send SMS (pseudo-code)
// await _smsSender.SendSmsAsync(user.PhoneNumber, "Registration successful on LendingSolution.");

// In Startup.cs or wherever you configure services
// builder.Services.AddScoped<IAuthService, AuthService>();
// builder.Services.AddScoped<IProfileService, ProfileService>();
// builder.Services.AddScoped<ILoanService, LoanService>();
// builder.Services.AddScoped<IRepaymentService, RepaymentService>();

// builder.Services.AddHttpContextAccessor();

// builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
//     .AddEntityFrameworkStores<ApplicationDbContext>()
//     .AddDefaultTokenProviders();