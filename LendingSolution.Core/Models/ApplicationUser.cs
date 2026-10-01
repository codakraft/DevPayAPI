using Microsoft.AspNetCore.Identity;

namespace LendingSolution.Core.Models;

public class ApplicationUser : IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Address { get; set; } = null;
    public string? City { get; set; } = null;
    public string? State { get; set; } = null;
    public string? CompanyId { get; set; } = null;
    public string? Gender { get; set; } = null;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // For accurate user registration analytics
    public bool IsActive { get; set; } = true; // User activation status
    public DateTime? LastLoginAt { get; set; }
    public bool RequiresPasswordChange { get; set; } = false; // Force password change on first login

    public DateTime? DateOfBirth { get; set; } // <-- Make sure you assign a DateTime, not a string

    /// <summary>
    /// Compares company ids as GUIDs. CompanyId is stored as a string and existing rows
    /// differ in letter case, so never compare it with == in memory.
    /// </summary>
    public bool BelongsToCompany(Guid companyId) =>
        Guid.TryParse(CompanyId, out var id) && id == companyId;

    public bool BelongsToCompany(string? companyId) =>
        Guid.TryParse(companyId, out var id) && BelongsToCompany(id);

}