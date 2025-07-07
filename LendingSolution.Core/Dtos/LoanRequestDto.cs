namespace LendingSolution.Core.Dtos
{
    public class LoanRequestDto
    {
        public Guid Id { get; set; }
        public string Bvn { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string SalaryBank { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
        public decimal AmountRequested { get; set; }
        public int DurationInMonths { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}