namespace LendingSolution.Core.Dtos
{
    public class DashboardDataDto
    {
        public PeriodData today { get; set; }
        public PeriodData thisMonth { get; set; }
        public PeriodData thisYear { get; set; }
        public PeriodData overall { get; set; }
    }

    public class PeriodData
    {
        public decimal totalRepaid { get; set; }
        public decimal totalDisbursed { get; set; }
    }
}