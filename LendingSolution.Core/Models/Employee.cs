using System;
using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Models
{
    public class Employee
    {
        public Guid Id { get; set; }
        public required string UserId { get; set; } // FK to ApplicationUser
        public required string Employer { get; set; }
        public required string Industry { get; set; }
        public required string Role { get; set; }
        public required string ResidentialAddress { get; set; }
        public Guid LoanId { get; set; } // FK to Loan
    }
}
