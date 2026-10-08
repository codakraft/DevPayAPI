using System.Security.Claims;
using LendingSolution.Application.Services.Interfaces;

namespace LendingSolution.API.Auth;

/// <summary>
/// Company (tenant) scoping helpers. Permissions say what a user can do;
/// these say which company's data they can do it to. Only SuperAdmin crosses companies.
/// </summary>
public static class TenantAccessExtensions
{
    public static bool IsSuperAdmin(this ClaimsPrincipal user) => user.IsInRole("SuperAdmin");

    public static Guid? GetCompanyId(this ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue("CompanyId"), out var id) ? id : null;

    public static bool CanAccessCompany(this ClaimsPrincipal user, Guid companyId) =>
        user.IsSuperAdmin() || user.GetCompanyId() == companyId;

    public static async Task<bool> CanAccessLoanAsync(this ClaimsPrincipal user, ILoanService loanService, Guid loanId)
    {
        if (user.IsSuperAdmin())
        {
            return true;
        }

        var loanCompanyId = await loanService.GetLoanCompanyIdAsync(loanId);
        return loanCompanyId.HasValue && user.CanAccessCompany(loanCompanyId.Value);
    }

    public static async Task<bool> CanAccessMonoMandateAsync(this ClaimsPrincipal user, IMonoService monoService, string mandateId)
    {
        if (user.IsSuperAdmin())
        {
            return true;
        }

        var mandateCompanyId = await monoService.GetMandateCompanyIdAsync(mandateId);
        return mandateCompanyId.HasValue && user.CanAccessCompany(mandateCompanyId.Value);
    }
}
