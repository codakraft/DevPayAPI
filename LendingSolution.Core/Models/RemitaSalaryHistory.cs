using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LendingSolution.Core.Models;

/// <summary>
/// Stores Remita salary history information for borrowers
/// </summary>
[Table("RemitaSalaryHistories")]
public class RemitaSalaryHistory
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the borrower application
    /// </summary>
    [Required]
    public Guid BorrowerApplicationId { get; set; }

    /// <summary>
    /// Customer ID from Remita
    /// </summary>
    [Required]
    [MaxLength(50)]
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Account number
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Bank code
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string BankCode { get; set; } = string.Empty;

    /// <summary>
    /// BVN from Remita response
    /// </summary>
    [MaxLength(20)]
    public string? BVN { get; set; }

    /// <summary>
    /// Company name from salary records
    /// </summary>
    [MaxLength(200)]
    public string? CompanyName { get; set; }

    /// <summary>
    /// Customer name from Remita
    /// </summary>
    [MaxLength(200)]
    public string? CustomerName { get; set; }

    /// <summary>
    /// Employee category
    /// </summary>
    [MaxLength(100)]
    public string? Category { get; set; }

    /// <summary>
    /// First payment date
    /// </summary>
    public DateTime? FirstPaymentDate { get; set; }

    /// <summary>
    /// Total count of salary payments
    /// </summary>
    public int SalaryCount { get; set; }

    /// <summary>
    /// Average monthly salary calculated from payment details
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal AverageMonthlySalary { get; set; }

    /// <summary>
    /// Latest salary amount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal LatestSalaryAmount { get; set; }

    /// <summary>
    /// Date of latest salary payment
    /// </summary>
    public DateTime? LatestPaymentDate { get; set; }

    /// <summary>
    /// Minimum salary amount in the history
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal MinSalaryAmount { get; set; }

    /// <summary>
    /// Maximum salary amount in the history
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal MaxSalaryAmount { get; set; }

    /// <summary>
    /// Number of months with consistent salary payments
    /// </summary>
    public int ConsistentMonths { get; set; }

    /// <summary>
    /// Has outstanding loans flag
    /// </summary>
    public bool HasOutstandingLoans { get; set; }

    /// <summary>
    /// Total outstanding loan amount
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalOutstandingAmount { get; set; }

    /// <summary>
    /// Raw JSON response from Remita for audit purposes
    /// </summary>
    [Column(TypeName = "nvarchar(max)")]
    public string? RawRemitaResponse { get; set; }

    /// <summary>
    /// When this record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this record was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual BorrowerApplication? BorrowerApplication { get; set; }

    // Collection for salary payment details
    public virtual ICollection<RemitaSalaryPayment> SalaryPayments { get; set; } = new List<RemitaSalaryPayment>();
}

/// <summary>
/// Individual salary payment record
/// </summary>
[Table("RemitaSalaryPayments")]
public class RemitaSalaryPayment
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Reference to the salary history record
    /// </summary>
    [Required]
    public Guid RemitaSalaryHistoryId { get; set; }

    /// <summary>
    /// Payment date
    /// </summary>
    [Required]
    public DateTime PaymentDate { get; set; }

    /// <summary>
    /// Payment amount
    /// </summary>
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>
    /// Account number for this payment
    /// </summary>
    [Required]
    [MaxLength(20)]
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Bank code for this payment
    /// </summary>
    [Required]
    [MaxLength(10)]
    public string BankCode { get; set; } = string.Empty;

    /// <summary>
    /// When this record was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public virtual RemitaSalaryHistory? RemitaSalaryHistory { get; set; }
}



    /// <summary>
    /// Loan provider name
    /// </summary>
