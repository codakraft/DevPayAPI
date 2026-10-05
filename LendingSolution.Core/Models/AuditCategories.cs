namespace LendingSolution.Core.Models;

/// <summary>
/// The fixed set of <see cref="AuditLog.Category"/> values. Clients filter on these exactly.
/// </summary>
public static class AuditCategories
{
    /// <summary>Successful sign-in steps and sign-out</summary>
    public const string Authentication = "Authentication";

    /// <summary>Failed sign-ins, OTP failures, password changes and resets, role and access changes</summary>
    public const string Security = "Security";

    /// <summary>Users created or edited</summary>
    public const string User = "User";

    /// <summary>Loan approval, rejection, disbursement and borrower onboarding milestones</summary>
    public const string Loan = "Loan";

    /// <summary>Wallet debits, credits and transfers</summary>
    public const string Financial = "Financial";

    /// <summary>Every category with its display label, in the order a filter should list them</summary>
    public static readonly IReadOnlyList<(string Value, string Label)> All =
    [
        (Authentication, "Logins"),
        (Security, "Security"),
        (User, "User Actions"),
        (Loan, "Loan Actions"),
        (Financial, "Wallet & Finance")
    ];
}
