namespace LendingSolution.Core.Dtos
{
    public class UpcomingCollectionDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string AccountNumber { get; set; }
        public DateTime DueDate { get; set; }
        public decimal Principal { get; set; }
        public decimal Interest { get; set; }
        public decimal Fee { get; set; }
        public decimal Total { get; set; }
    }
}