namespace LendingSolution.Core.Dtos
{
    public class LoanProductUpdateDto
    {
        public string Name { get; set; }
        public decimal? InterestRate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public int? MinTenure { get; set; }
        public int? MaxTenure { get; set; }
        public string Status { get; set; }
        public int? Moratorium { get; set; }
    }
}