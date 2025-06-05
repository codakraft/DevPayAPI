using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos
{
    public class ReviewHistoryRequestDto
    {
        [Required]
        [RegularExpression("^\\d{10}$", ErrorMessage = "Account number must be exactly 10 digits.")]
        public required string AccountNumber { get; set; }

        [Required]
        public required string BankCode { get; set; }
    }
}
