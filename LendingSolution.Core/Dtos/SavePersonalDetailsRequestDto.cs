using System;
using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos
{
    public class SavePersonalDetailsRequestDto
    {
        [Required]
        public required string Employer { get; set; }

        [Required]
        public required string Industry { get; set; }

        [Required]
        public required string Role { get; set; }

        [Required]
        public required string ResidentialAddress { get; set; }

        [Required]
        public Guid LoanId { get; set; }
    }
}
