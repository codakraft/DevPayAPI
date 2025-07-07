using Microsoft.AspNetCore.Identity;

namespace LendingSolution.Core.Models;

public class ApplicationUser : IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Address { get; set; } = null;
    public string? City { get; set; } = null;
    public string? State { get; set; } = null;
    public string? CompanyId { get; set; } // For multi-tenant support
    public string? Gender { get; set; } = null; // Male, Female, Other, PreferNotToSay
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // For accurate user registration analytics
    public bool IsActive { get; set; } = true; // User activation status
    public DateTime? LastLoginAt { get; set; } // Last login timestamp

    public DateTime? DateOfBirth { get; set; } // <-- Make sure you assign a DateTime, not a string

}