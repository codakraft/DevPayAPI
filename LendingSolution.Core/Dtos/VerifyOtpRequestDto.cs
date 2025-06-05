using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos
{
    public class VerifyOtpRequestDto
    {
        [Required]
        public Guid LoanId { get; set; }

        [Required]
        public required string Otp { get; set; }
    }
}
