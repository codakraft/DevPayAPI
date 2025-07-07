using LendingSolution.Core.Models;
using LendingSolution.Core.Dtos.Response;

namespace LendingSolution.Application.Repositories.Interfaces;

public interface IApprovalRepository
{
    Task<IEnumerable<Approval>> GetAllApprovalsAsync();
    Task<IEnumerable<Approval>> GetPendingApprovalsAsync();
    Task<IEnumerable<Approval>> GetApprovalsByStatusAsync(string status);
    Task<IEnumerable<Approval>> GetApprovalsByTypeAsync(string approvalType);
    Task<IEnumerable<Approval>> GetApprovalsByCompanyAsync(string companyId);
    Task<Approval?> GetApprovalByIdAsync(string id);
    Task<ApiResponse> CreateApprovalAsync(Approval approval);
    Task<ApiResponse> UpdateApprovalAsync(Approval approval);
    Task<ApiResponse> DeleteApprovalAsync(string id);
    Task<bool> ApprovalExistsAsync(string id);
}
