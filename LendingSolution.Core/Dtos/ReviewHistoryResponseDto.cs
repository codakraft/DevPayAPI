namespace LendingSolution.Core.Dtos
{
    public class ReviewHistoryResponseDto
    {
        public required string CompanyName { get; set; }
        public decimal MaxEligibleAmount { get; set; }
    }
}
