namespace LendingSolution.Core.Dtos;

/// <summary>
/// DTO for wallet information
/// </summary>
public class WalletDto
{
    public Guid Id { get; set; }
    public Guid? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public decimal Balance { get; set; }
    public decimal TotalCredits { get; set; }
    public decimal TotalDebits { get; set; }
    public bool IsSuperAdminWallet { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO for wallet transaction information
/// </summary>
public class WalletTransactionDto
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public string? InitiatedBy { get; set; }
    public string? PaystackReference { get; set; }

    /// <summary>Pending, Completed or Failed for Paystack fundings; null for other transactions</summary>
    public string? Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for funding wallet via Paystack
/// </summary>
public class FundWalletDto
{
    public Guid WalletId { get; set; }
    public decimal Amount { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? CallbackUrl { get; set; }
}

/// <summary>
/// DTO for Paystack payment initialization response
/// </summary>
public class PaystackInitializationDto
{
    public bool Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public PaystackAuthorizationData? Data { get; set; }
}

/// <summary>
/// What Paystack reports for a transaction when it is verified
/// </summary>
public class PaystackVerificationResult
{
    /// <summary>False when Paystack couldn't be reached or didn't recognise the reference</summary>
    public bool Verified { get; set; }

    /// <summary>Paystack's transaction status: success, failed, abandoned, ...</summary>
    public string? Status { get; set; }

    /// <summary>Amount paid, in kobo</summary>
    public long AmountKobo { get; set; }
    public string? Currency { get; set; }
}

/// <summary>
/// Result of completing a Paystack wallet funding
/// </summary>
public class WalletFundingResultDto
{
    /// <summary>Paystack's transaction status; "success" when the wallet was credited</summary>
    public string TransactionStatus { get; set; } = string.Empty;
    public decimal Amount { get; set; }

    /// <summary>Wallet balance after this funding</summary>
    public decimal Balance { get; set; }

    /// <summary>True when this reference had already been credited; nothing was credited again</summary>
    public bool AlreadyCompleted { get; set; }
}

public class PaystackAuthorizationData
{
    public string AuthorizationUrl { get; set; } = string.Empty;
    public string AccessCode { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
}

/// <summary>
/// DTO for wallet transaction creation
/// </summary>
public class CreateWalletTransactionDto
{
    public Guid WalletId { get; set; }
    public decimal Amount { get; set; }
    public int TransactionType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public string? PaystackReference { get; set; }
}

/// <summary>
/// DTO for debit wallet request
/// </summary>
public class DebitWalletDto
{
    public Guid WalletId { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ReferenceId { get; set; }
    public int TransactionType { get; set; }
}

/// <summary>
/// DTO for wallet transaction query
/// </summary>
public class WalletTransactionQueryDto
{
    public Guid? WalletId { get; set; }
    public int? TransactionType { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// DTO for wallet report
/// </summary>
public class WalletReportDto
{
    public Guid WalletId { get; set; }
    public string WalletName { get; set; } = string.Empty;
    public decimal CurrentBalance { get; set; }
    public decimal TotalCredits { get; set; }
    public decimal TotalDebits { get; set; }
    public decimal TotalFees { get; set; }
    public int TransactionCount { get; set; }
    public DateTime ReportPeriodFrom { get; set; }
    public DateTime ReportPeriodTo { get; set; }
    public List<WalletTransactionDto> RecentTransactions { get; set; } = new();
}

/// <summary>
/// DTO for completing wallet funding
/// </summary>
public class CompleteFundingDto
{
    public string PaystackReference { get; set; } = string.Empty;
}
