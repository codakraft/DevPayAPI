using System;

namespace LendingSolution.Core.Models;

public class Loan
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }
    public decimal Amount { get; set; }
    public int DurationInMonths { get; set; }
    public required string Purpose { get; set; }
    public required string Status { get; set; } // e.g., Pending, Submitted, Approved, Rejected, Disbursed, Completed
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? RejectedAt { get; set; }
}