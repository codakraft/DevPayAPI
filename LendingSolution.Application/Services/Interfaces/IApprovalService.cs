using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;

namespace LendingSolution.Application.Services.Interfaces;

public interface IApprovalService
{
    Task<ApiResponse> GetAllApprovalsAsync();
    Task<ApiResponse> GetPendingApprovalsAsync();
    Task<ApiResponse> GetApprovalsByStatusAsync(string status);
    Task<ApiResponse> GetApprovalsByTypeAsync(string approvalType);
    Task<ApiResponse> GetApprovalByIdAsync(string id);
    Task<ApiResponse> CreateApprovalAsync(ApprovalRequestDto approvalDto, string requestedBy, string? companyId = null);
    Task<ApiResponse> ApproveRequestAsync(string approvalId, string processedBy, string? reason = null);
    Task<ApiResponse> RejectRequestAsync(string approvalId, string processedBy, string? reason = null);
    Task<ApiResponse> ProcessApprovalAsync(ProcessApprovalDto processDto, string processedBy);
}
