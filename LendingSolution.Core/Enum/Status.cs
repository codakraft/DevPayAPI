namespace LendingSolution.Core.Enum;

public enum LoanStatus
{
    Pending = 0,
    NotBooked = 1,
    Approved = 2,
    Rejected = 3,
    Disbursed = 4,
    Repaid = 5,
    Overdue = 6,
    Cancelled = 7
}

public enum InterestComputationBasis
{
    Flat = 0,
    Reducing = 1
}

public enum InterestCostComputation
{
    PerMonth = 0,
    PerYear = 1
}

public enum PaymentScheduleBreakdown
{
    Monthly = 0,
    BiWeekly = 1,
    Weekly = 2
}

public enum PaymentScheduleType
{
    Fixed = 0,
    Variable = 1
}
