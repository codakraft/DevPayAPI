using Microsoft.AspNetCore.Identity;

namespace LendingSolution.Core.Models;

public class ApplicationUser : IdentityUser
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Address { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }

    public DateTime? DateOfBirth { get; set; } // <-- Make sure you assign a DateTime, not a string

}