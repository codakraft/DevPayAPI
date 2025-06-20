namespace LendingSolution.Core.Dtos
{
    public class LoanEligibilityDto
    {
        public DateTime RequestDate { get; set; }
        public string Bvn { get; set; }
        public string Email { get; set; }
        public string SalaryBank { get; set; }
        public string AccountNumber { get; set; }
        public decimal AmountRequested { get; set; }
        public int DurationInMonths { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
    }
}