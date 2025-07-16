using LendingSolution.Core.Models;
using LendingSolution.Core.Enum;

namespace LendingSolution.Core.Models;

public class LoanProduct : Base
{
    public required Guid CompanyId { get; set; }
    public Company Company { get; set; } = default!;
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required string Description { get; set; }
    public required string ShortName { get; set; }
    
    // Amount and Tenor ranges
    public decimal MinAmount { get; set; } = 0;
    public decimal MaxAmount { get; set; } = 0;
    public int MinTenor { get; set; } = 0;
    public int MaxTenor { get; set; } = 0;
    
    // Interest and fees
    public decimal InterestRate { get; set; } = 0;
    public decimal PenaltyOnDefaultPrincipal { get; set; } = 0;
    
    // Eligibility and settings
    public int Moratorium { get; set; } = 30; // in days
    public bool NotifyApprovalsViaEmail { get; set; } = false;
    public decimal TurnoverEligibilityPercent { get; set; } = 0;
    
    // Interest computation settings
    public InterestComputationBasis InterestComputationBasis { get; set; } = InterestComputationBasis.Flat;
    public InterestCostComputation InterestCostComputation { get; set; } = InterestCostComputation.PerMonth;
    
    // Payment schedule settings
    public PaymentScheduleBreakdown PaymentScheduleBreakdown { get; set; } = PaymentScheduleBreakdown.Monthly;
    public PaymentScheduleType PaymentScheduleType { get; set; } = PaymentScheduleType.Fixed;
    
    // Status
    public bool IsActive { get; set; } = false;
    
    // Navigation properties
    public ICollection<Loan> Loans { get; set; } = [];
}