using System;
using LendingSolution.Core.Models;

namespace LendingSolution.Core.Models
{
    public class Repayment
    {
        public Guid Id { get; set; }
        public Guid LoanId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaidAt { get; set; }

        // Navigation property (optional)
        public Loan? Loan { get; set; }
    }
}