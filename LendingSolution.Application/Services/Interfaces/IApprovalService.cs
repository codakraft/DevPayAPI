using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;

namespace LendingSolution.Application.Services.Interfaces;

public interface IApprovalService
{
    Task<List<ApprovalDto>> GetAllApprovalsAsync();
    Task<List<ApprovalDto>> GetPendingApprovalsAsync();
    Task<List<ApprovalDto>> GetApprovalsByStatusAsync(string status);
    Task<List<ApprovalDto>> GetApprovalsByTypeAsync(string approvalType);
    Task<ApprovalDto> GetApprovalByIdAsync(string id);
    Task<ApprovalDto> CreateApprovalAsync(ApprovalRequestDto approvalDto, string requestedBy, string? companyId = null);
    Task<ApprovalDto> ApproveRequestAsync(string approvalId, string processedBy, string? reason = null);
    Task<ApprovalDto> RejectRequestAsync(string approvalId, string processedBy, string? reason = null);
    Task<ApprovalDto> ProcessApprovalAsync(ProcessApprovalDto processDto, string processedBy);
}
