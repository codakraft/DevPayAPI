namespace LendingSolution.Core.Dtos
{
    public class OngoingCollectionDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string AccountNo { get; set; }
        public decimal LoanAmount { get; set; }
        public DateTime RequestDate { get; set; }
        public int TenorInMonths { get; set; }
        public decimal UnpaidPrincipal { get; set; }
        public decimal UnpaidInterest { get; set; }
        public decimal TotalUnpaid { get; set; }
    }
}