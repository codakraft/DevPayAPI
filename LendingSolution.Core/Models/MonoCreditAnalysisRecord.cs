using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Models;

/// <summary>
/// Credit analysis results - temporary storage only (30 days max)
/// Does NOT store raw credit data for privacy compliance
/// Only stores computed analysis results needed for loan decisions
/// </summary>
public class MonoCreditAnalysisRecord
{
    public Guid Id { get; set; }
    public Guid BorrowerApplicationId { get; set; }
    
    [Required]
    [MaxLength(64)]
    public string BvnHash { get; set; } = string.Empty; // SHA-256 hash for privacy
    
    [Required]
    [MaxLength(10)]
    public string Provider { get; set; } = string.Empty; // 'xds' or 'cdc'
    
    // Analysis results only (not raw credit data)
    public decimal CreditScore { get; set; } // 0-1000 scale
    public decimal MaxLoanAmount { get; set; }
    
    [MaxLength(20)]
    public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High
    
    public int ActiveLoansCount { get; set; }
    public decimal TotalOutstandingDebt { get; set; }
    
    [MaxLength(50)]
    public string OverallPerformanceStatus { get; set; } = string.Empty; // performing, non-performing, etc.
    
    [MaxLength(20)]
    public string RecommendedAction { get; set; } = string.Empty; // Approve, Review, Decline

    // Computed narrative factors (not raw credit data), stored as JSON arrays.
    public string? RiskFactorsJson { get; set; }
    public string? PositiveFactorsJson { get; set; }

    // Auto-deletion for data protection compliance
    public DateTime ExpiresAt { get; set; } // Delete after 30 days
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation property
    public BorrowerApplication BorrowerApplication { get; set; } = null!;
}