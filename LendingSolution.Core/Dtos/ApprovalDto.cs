namespace LendingSolution.Core.Dtos;

public class ApprovalDto
{
    public string Id { get; set; } = string.Empty;
    public string ApprovalType { get; set; } = string.Empty; // Loan, User, Company, etc.
    public string ReferenceId { get; set; } = string.Empty; // ID of the item being approved
    public string RequestedBy { get; set; } = string.Empty;
    public string RequestedByName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; // Pending, Approved, Rejected
    public string? Description { get; set; }
    public string? Reason { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ProcessedBy { get; set; }
    public string? ProcessedByName { get; set; }
}

public class ApprovalRequestDto
{
    public required string ApprovalType { get; set; }
    public required string ReferenceId { get; set; }
    public string? Description { get; set; }
}

public class ProcessApprovalDto
{
    public required string ApprovalId { get; set; }
    public required string Action { get; set; } // "approve" or "reject"
    public string? Reason { get; set; }
}
