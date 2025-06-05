using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos
{
    public class LoanBreakdownRequestDto
    {
        [Required]
        public decimal Amount { get; set; }

        [Required]
        public int DurationInMonths { get; set; }
    }
}
