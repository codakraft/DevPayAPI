using System;
using System.Collections.Generic;

namespace LendingSolution.Core.Dtos
{
    public class LoanBreakdownResponseDto
    {
        public decimal LoanAmount { get; set; }
        public int Tenor { get; set; } // Duration in months
        public DateTime NextRepaymentDate { get; set; }
        public required List<RepaymentScheduleDto> RepaymentSchedules { get; set; }
    }

    public class RepaymentScheduleDto
    {
        public DateTime RepaymentDate { get; set; }
        public decimal Amount { get; set; }
    }
}
