using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Models;

/// <summary>
/// Stores Remita customer authorization codes and customer IDs per borrower application
/// </summary>
public class RemitaCustomer : Base
{
    /// <summary>
    /// Customer ID returned from Remita (unique per borrower application)
    /// </summary>
    [MaxLength(100)]
    public string CustomerId { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the borrower application
    /// </summary>
    [Required]
    public Guid BorrowerApplicationId { get; set; }

    /// <summary>
    /// Authorization code generated for each salary history request
    /// </summary>
    [MaxLength(20)]
    public string AuthorisationCode { get; set; } = string.Empty;

    /// <summary>
    /// Last time this record was used
    /// </summary>
    public DateTime? LastUsedAt { get; set; }
}
