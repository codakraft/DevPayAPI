using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

public class SupportTicketDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Account, Loan, Payment, Technical, Other
    public string Priority { get; set; } = string.Empty; // Low, Medium, High, Critical
    public string Status { get; set; } = string.Empty; // Open, InProgress, Resolved, Closed
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public List<SupportCommentDto> Comments { get; set; } = new();
}

public class CreateSupportTicketDto
{
    [Required]
    [MaxLength(200)]
    public string Subject { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public string Category { get; set; } = string.Empty;
    
    public string Priority { get; set; } = "Medium";
}

public class UpdateSupportTicketDto
{
    public string? Status { get; set; }
    public string? AssignedTo { get; set; }
    public string? Priority { get; set; }
}

public class SupportCommentDto
{
    public string Id { get; set; } = string.Empty;
    public string TicketId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public bool IsInternal { get; set; } = false;
    public DateTime CreatedAt { get; set; }
}

public class AddSupportCommentDto
{
    [Required]
    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;
    
    public bool IsInternal { get; set; } = false;
}

public class UserAccountSupportDto
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public int TotalLoans { get; set; }
    public int ActiveLoans { get; set; }
    public decimal TotalLoanAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
}

public class LoanSupportDto
{
    public string LoanId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int DurationInMonths { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public decimal TotalDisbursed { get; set; }
    public decimal TotalRepaid { get; set; }
    public decimal OutstandingAmount { get; set; }
}

public class SupportDashboardDto
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int HighPriorityTickets { get; set; }
    public int CriticalPriorityTickets { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public List<SupportTicketDto> RecentTickets { get; set; } = new();
}

public class UserActionDto
{
    [Required]
    public string Action { get; set; } = string.Empty; // Suspend, Activate, ResetPassword, etc.
    
    [MaxLength(500)]
    public string? Reason { get; set; }
}

public class CompanyDashboardDto
{
    public UserStatistics Users { get; set; } = new();
    public LoanStatistics Loans { get; set; } = new();
    public UserAnalytics Analytics { get; set; } = new();
    public DisbursementAnalytics Disbursements { get; set; } = new();
    public LoanRequestAnalytics LoanRequests { get; set; } = new();
    public FinancialMetrics FinancialMetrics { get; set; } = new();
    public SystemMetrics SystemMetrics { get; set; } = new();
}

public class UserStatistics
{
    public int Today { get; set; }
    public int ThisWeek { get; set; }
    public int ThisMonth { get; set; }
    public int ThisYear { get; set; }
    public int AllTime { get; set; }
}

public class LoanStatistics
{
    public int Today { get; set; }
    public int ThisWeek { get; set; }
    public int ThisMonth { get; set; }
    public int ThisYear { get; set; }
    public int AllTime { get; set; }
}

public class UserAnalytics
{
    public double MalePercentage { get; set; }
    public double FemalePercentage { get; set; }
    public double OtherPercentage { get; set; }
    public int TotalUsers { get; set; }
}

public class DisbursementAnalytics
{
    public List<GraphDataPoint> Last7Days { get; set; } = new();
    public List<GraphDataPoint> CurrentMonthDaily { get; set; } = new();
    public List<GraphDataPoint> CurrentMonthWeekly { get; set; } = new();
    public List<GraphDataPoint> CurrentYearMonthly { get; set; } = new();
}

public class LoanRequestAnalytics
{
    public List<GraphDataPoint> Last7Days { get; set; } = new();
    public List<GraphDataPoint> CurrentMonthDaily { get; set; } = new();
    public List<GraphDataPoint> CurrentMonthWeekly { get; set; } = new();
    public List<GraphDataPoint> CurrentYearMonthly { get; set; } = new();
}

public class GraphDataPoint
{
    public string Label { get; set; } = string.Empty; // Date label
    public decimal Value { get; set; } // Amount or count
    public DateTime Date { get; set; } // Actual date for sorting/filtering
}

public class FinancialMetrics
{
    public decimal TotalDisbursed { get; set; }
    public decimal TotalRequested { get; set; }
    public decimal AverageRequestAmount { get; set; }
    public decimal AverageDisbursementAmount { get; set; }
    public decimal OutstandingBalance { get; set; }
    public decimal TotalRepaid { get; set; }
    public double RepaymentRate { get; set; } // Percentage
    public int DefaultedLoans { get; set; }
    public double DefaultRate { get; set; } // Percentage
}

public class SystemMetrics
{
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public double UserActivityRate { get; set; } // Percentage
    public int PendingApprovals { get; set; }
    public int ApprovedLoans { get; set; }
    public int RejectedLoans { get; set; }
    public double ApprovalRate { get; set; } // Percentage
    public int OpenSupportTickets { get; set; }
    public double TicketResolutionRate { get; set; } // Percentage
}

public class EnhancedUserAnalytics
{
    public GenderDistribution GenderBreakdown { get; set; } = new();
    public AgeDistribution AgeBreakdown { get; set; } = new();
    public List<GraphDataPoint> UserRegistrationTrend { get; set; } = new();
    public List<GraphDataPoint> UserActivityTrend { get; set; } = new();
}

public class GenderDistribution
{
    public int MaleCount { get; set; }
    public int FemaleCount { get; set; }
    public int OtherCount { get; set; }
    public int PreferNotToSayCount { get; set; }
    public double MalePercentage { get; set; }
    public double FemalePercentage { get; set; }
    public double OtherPercentage { get; set; }
    public double PreferNotToSayPercentage { get; set; }
    public int TotalUsers { get; set; }
}

public class AgeDistribution
{
    public int Under25 { get; set; }
    public int Age25To34 { get; set; }
    public int Age35To44 { get; set; }
    public int Age45To54 { get; set; }
    public int Age55Plus { get; set; }
    public int UnknownAge { get; set; }
    public double Under25Percentage { get; set; }
    public double Age25To34Percentage { get; set; }
    public double Age35To44Percentage { get; set; }
    public double Age45To54Percentage { get; set; }
    public double Age55PlusPercentage { get; set; }
    public double UnknownAgePercentage { get; set; }
}

public class SuperAdminDashboardDto
{
    public SystemWideStatistics SystemStats { get; set; } = new();
    public List<CompanyOverviewDto> Companies { get; set; } = new();
    public SystemWideAnalytics Analytics { get; set; } = new();
    public SystemWideFinancialMetrics FinancialMetrics { get; set; } = new();
    public PlatformPerformanceMetrics Performance { get; set; } = new();
    public List<GraphDataPoint> CompanyGrowthTrend { get; set; } = new();
    public List<GraphDataPoint> UserGrowthTrend { get; set; } = new();
    public List<GraphDataPoint> LoanVolumeTrend { get; set; } = new();
    public List<GraphDataPoint> RevenueGrowthTrend { get; set; } = new();
}

public class SystemWideStatistics
{
    public int TotalCompanies { get; set; }
    public int ActiveCompanies { get; set; }
    public int TotalUsers { get; set; }
    public int TotalLoans { get; set; }
    public int TodayUsers { get; set; }
    public int TodayLoans { get; set; }
    public int ThisWeekUsers { get; set; }
    public int ThisWeekLoans { get; set; }
    public int ThisMonthUsers { get; set; }
    public int ThisMonthLoans { get; set; }
    public int ThisYearUsers { get; set; }
    public int ThisYearLoans { get; set; }
}

public class CompanyOverviewDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int TotalUsers { get; set; }
    public int TotalLoans { get; set; }
    public decimal TotalLoanAmount { get; set; }
    public decimal TotalDisbursed { get; set; }
    public double DefaultRate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActivity { get; set; }
}

public class SystemWideAnalytics
{
    public GenderDistribution UserGenderBreakdown { get; set; } = new();
    public LoanStatusDistribution LoanStatusBreakdown { get; set; } = new();
    public CompanyPerformanceMetrics TopPerformingCompanies { get; set; } = new();
    public List<CompanyRiskMetrics> CompanyRiskAnalysis { get; set; } = new();
}

public class LoanStatusDistribution
{
    public int PendingLoans { get; set; }
    public int ApprovedLoans { get; set; }
    public int DisbursedLoans { get; set; }
    public int RepaidLoans { get; set; }
    public int OverdueLoans { get; set; }
    public int RejectedLoans { get; set; }
    public double PendingPercentage { get; set; }
    public double ApprovedPercentage { get; set; }
    public double DisbursedPercentage { get; set; }
    public double RepaidPercentage { get; set; }
    public double OverduePercentage { get; set; }
    public double RejectedPercentage { get; set; }
}

public class CompanyPerformanceMetrics
{
    public List<CompanyPerformanceDto> ByLoanVolume { get; set; } = new();
    public List<CompanyPerformanceDto> ByUserCount { get; set; } = new();
    public List<CompanyPerformanceDto> ByRevenue { get; set; } = new();
    public List<CompanyPerformanceDto> ByGrowthRate { get; set; } = new();
}

public class CompanyPerformanceDto
{
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public double Percentage { get; set; }
    public string Metric { get; set; } = string.Empty;
}

public class CompanyRiskMetrics
{
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public double DefaultRate { get; set; }
    public double OverdueRate { get; set; }
    public int TotalLoans { get; set; }
    public decimal ExposureAmount { get; set; }
    public string RiskLevel { get; set; } = "Low"; // Low, Medium, High, Critical
}

public class SystemWideFinancialMetrics
{
    public decimal TotalLoanRequests { get; set; }
    public decimal TotalDisbursed { get; set; }
    public decimal TotalRepayments { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal AverageLoanSize { get; set; }
    public double SystemRepaymentRate { get; set; }
    public double SystemDefaultRate { get; set; }
    public decimal MonthlyDisbursementVolume { get; set; }
    public decimal MonthlyRepaymentVolume { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueThisYear { get; set; }
}

public class PlatformPerformanceMetrics
{
    public int TotalSupportTickets { get; set; }
    public int OpenTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public double TicketResolutionRate { get; set; }
    public double AverageResolutionTimeHours { get; set; }
    public int SystemUptime { get; set; }
    public double UserSatisfactionScore { get; set; }
    public int TotalTransactions { get; set; }
    public double TransactionSuccessRate { get; set; }
}
