using LendingSolution.Core.Enum;

namespace LendingSolution.Core.Models;

public class Repayment : Base
{
    public Guid LoanId { get; set; }
    public Loan? Loan { get; set; }
    
    public decimal TotalDue { get; set; }
    public decimal TotalRepaid { get; set; } = 0;
    public DateTime? LastPaymentAt { get; set; }
    public decimal AmountUnpaid { get; set; }
    public RepaymentStatus Status { get; set; } = RepaymentStatus.Active;
}