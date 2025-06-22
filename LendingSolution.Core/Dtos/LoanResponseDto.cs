namespace LendingSolution.Core.Dtos;

public class LoanResponseDto : LoanDto
{
    public Guid Id { get; set; }
    public required string UserId { get; set; }

    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
    public DateTime? MandateCreatedAt { get; set; }
}