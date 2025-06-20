namespace  LendingSolution.Core.Dtos
{
    public class SuccessfulRepaymentDto
    {
        public string Id { get; set; }
        public string LoanId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AccountNumber { get; set; }
        public decimal Amount { get; set; }
        public decimal LoanAmount { get; set; }
        public int Tenor { get; set; }
        public decimal Fee { get; set; }
        public decimal Total { get; set; }
    }
}