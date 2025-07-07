using Microsoft.AspNetCore.Identity;
using LendingSolution.Core.Models;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Application.Repositories.Interfaces;

namespace LendingSolution.Application.Services.Implementations;

public class ApprovalService(
    IApprovalRepository approvalRepository,
    UserManager<ApplicationUser> userManager) : IApprovalService
{
    private readonly IApprovalRepository _approvalRepository = approvalRepository;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<ApiResponse> GetAllApprovalsAsync()
    {
        try
        {
            var approvals = await _approvalRepository.GetAllApprovalsAsync();
            
            var approvalDtos = approvals.Select(a => MapToDto(a)).ToList();

            return ApiResponse.Ok("Approvals retrieved successfully", approvalDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to retrieve approvals: {ex.Message}");
        }
    }

    public async Task<ApiResponse> GetPendingApprovalsAsync()
    {
        try
        {
            var pendingApprovals = await _approvalRepository.GetPendingApprovalsAsync();
            
            var approvalDtos = pendingApprovals.Select(a => MapToDto(a)).ToList();

            return ApiResponse.Ok("Pending approvals retrieved successfully", approvalDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to retrieve pending approvals: {ex.Message}");
        }
    }

    public async Task<ApiResponse> GetApprovalsByStatusAsync(string status)
    {
        try
        {
            var approvals = await _approvalRepository.GetApprovalsByStatusAsync(status);
            
            var approvalDtos = approvals.Select(a => MapToDto(a)).ToList();

            return ApiResponse.Ok($"Approvals with status '{status}' retrieved successfully", approvalDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to retrieve approvals: {ex.Message}");
        }
    }

    public async Task<ApiResponse> GetApprovalsByTypeAsync(string approvalType)
    {
        try
        {
            var approvals = await _approvalRepository.GetApprovalsByTypeAsync(approvalType);
            
            var approvalDtos = approvals.Select(a => MapToDto(a)).ToList();

            return ApiResponse.Ok($"Approvals of type '{approvalType}' retrieved successfully", approvalDtos);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to retrieve approvals: {ex.Message}");
        }
    }

    public async Task<ApiResponse> GetApprovalByIdAsync(string id)
    {
        try
        {
            var approval = await _approvalRepository.GetApprovalByIdAsync(id);
            
            if (approval == null)
            {
                return ApiResponse.Fail("Approval not found");
            }

            var approvalDto = MapToDto(approval);

            return ApiResponse.Ok("Approval retrieved successfully", approvalDto);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to retrieve approval: {ex.Message}");
        }
    }

    public async Task<ApiResponse> CreateApprovalAsync(ApprovalRequestDto approvalDto, string requestedBy, string? companyId = null)
    {
        try
        {
            Guid? companyGuid = null;
            if (companyId != null && Guid.TryParse(companyId, out var parsedGuid))
            {
                companyGuid = parsedGuid;
            }

            var approval = new Approval
            {
                ApprovalType = approvalDto.ApprovalType,
                ReferenceId = approvalDto.ReferenceId,
                RequestedBy = requestedBy,
                Description = approvalDto.Description,
                CompanyId = companyGuid
            };

            return await _approvalRepository.CreateApprovalAsync(approval);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to create approval: {ex.Message}");
        }
    }

    public async Task<ApiResponse> ApproveRequestAsync(string approvalId, string processedBy, string? reason = null)
    {
        try
        {
            var approval = await _approvalRepository.GetApprovalByIdAsync(approvalId);
            
            if (approval == null)
            {
                return ApiResponse.Fail("Approval not found");
            }

            if (approval.Status != "Pending")
            {
                return ApiResponse.Fail("Only pending approvals can be processed");
            }

            approval.Status = "Approved";
            approval.ProcessedBy = processedBy;
            approval.ProcessedAt = DateTime.UtcNow;
            approval.Reason = reason;

            return await _approvalRepository.UpdateApprovalAsync(approval);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to approve request: {ex.Message}");
        }
    }

    public async Task<ApiResponse> RejectRequestAsync(string approvalId, string processedBy, string? reason = null)
    {
        try
        {
            var approval = await _approvalRepository.GetApprovalByIdAsync(approvalId);
            
            if (approval == null)
            {
                return ApiResponse.Fail("Approval not found");
            }

            if (approval.Status != "Pending")
            {
                return ApiResponse.Fail("Only pending approvals can be processed");
            }

            approval.Status = "Rejected";
            approval.ProcessedBy = processedBy;
            approval.ProcessedAt = DateTime.UtcNow;
            approval.Reason = reason;

            return await _approvalRepository.UpdateApprovalAsync(approval);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to reject request: {ex.Message}");
        }
    }

    public async Task<ApiResponse> ProcessApprovalAsync(ProcessApprovalDto processDto, string processedBy)
    {
        try
        {
            if (processDto.Action.ToLower() == "approve")
            {
                return await ApproveRequestAsync(processDto.ApprovalId, processedBy, processDto.Reason);
            }
            else if (processDto.Action.ToLower() == "reject")
            {
                return await RejectRequestAsync(processDto.ApprovalId, processedBy, processDto.Reason);
            }
            else
            {
                return ApiResponse.Fail("Invalid action. Use 'approve' or 'reject'");
            }
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to process approval: {ex.Message}");
        }
    }

    private static ApprovalDto MapToDto(Approval approval)
    {
        return new ApprovalDto
        {
            Id = approval.Id,
            ApprovalType = approval.ApprovalType,
            ReferenceId = approval.ReferenceId,
            RequestedBy = approval.RequestedBy,
            RequestedByName = approval.RequestedByUser?.FirstName + " " + approval.RequestedByUser?.LastName ?? "",
            Status = approval.Status,
            Description = approval.Description,
            Reason = approval.Reason,
            RequestedAt = approval.RequestedAt,
            ProcessedAt = approval.ProcessedAt,
            ProcessedBy = approval.ProcessedBy,
            ProcessedByName = approval.ProcessedByUser?.FirstName + " " + approval.ProcessedByUser?.LastName ?? ""
        };
    }
}
