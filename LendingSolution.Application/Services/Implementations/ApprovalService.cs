using Microsoft.AspNetCore.Identity;
using LendingSolution.Core.Models;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Exceptions;

namespace LendingSolution.Application.Services.Implementations;

public class ApprovalService(
    IApprovalRepository approvalRepository,
    UserManager<ApplicationUser> userManager) : IApprovalService
{
    private readonly IApprovalRepository _approvalRepository = approvalRepository;
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task<List<ApprovalDto>> GetAllApprovalsAsync()
    {
        var approvals = await _approvalRepository.GetAllApprovalsAsync();
        
        var approvalDtos = approvals.Select(a => MapToDto(a)).ToList();

        return approvalDtos;
    }

    public async Task<List<ApprovalDto>> GetPendingApprovalsAsync()
    {
        var pendingApprovals = await _approvalRepository.GetPendingApprovalsAsync();
        
        var approvalDtos = pendingApprovals.Select(a => MapToDto(a)).ToList();

        return approvalDtos;
    }

    public async Task<List<ApprovalDto>> GetApprovalsByStatusAsync(string status)
    {
        var approvals = await _approvalRepository.GetApprovalsByStatusAsync(status);
        
        var approvalDtos = approvals.Select(a => MapToDto(a)).ToList();

        return approvalDtos;
    }

    public async Task<List<ApprovalDto>> GetApprovalsByTypeAsync(string approvalType)
    {
        var approvals = await _approvalRepository.GetApprovalsByTypeAsync(approvalType);
        
        var approvalDtos = approvals.Select(a => MapToDto(a)).ToList();

        return approvalDtos;
    }

    public async Task<ApprovalDto> GetApprovalByIdAsync(string id)
    {
        var approval = await _approvalRepository.GetApprovalByIdAsync(id) ?? throw new AppException("Approval not found", 404);
        var approvalDto = MapToDto(approval);

        return approvalDto;
    }

    public async Task<ApprovalDto> CreateApprovalAsync(ApprovalRequestDto approvalDto, string requestedBy, string? companyId = null)
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

        var result = await _approvalRepository.CreateApprovalAsync(approval);
        
        // Check if the repository returns an ApiResponse (legacy)
        if (result is ApiResponse apiResponse)
        {
            if (!apiResponse.Success)
            {
                throw new AppException(apiResponse.Message);
            }
            
            // Get the created approval
            var createdApproval = await _approvalRepository.GetApprovalByIdAsync(approval.Id);
            if (createdApproval == null)
            {
                throw new AppException("Failed to retrieve created approval");
            }
            
            return MapToDto(createdApproval);
        }
        
        // If repository returns the approval directly
        return MapToDto(approval);
    }

    public async Task<ApprovalDto> ApproveRequestAsync(string approvalId, string processedBy, string? reason = null)
    {
        var approval = await _approvalRepository.GetApprovalByIdAsync(approvalId) ?? throw new AppException("Approval not found", 404);
        if (approval.Status != "Pending")
        {
            throw new AppException("Only pending approvals can be processed", 400);
        }

        approval.Status = "Approved";
        approval.ProcessedBy = processedBy;
        approval.ProcessedAt = DateTime.UtcNow;
        approval.Reason = reason;

        var result = await _approvalRepository.UpdateApprovalAsync(approval);
        
        // Check if the repository returns an ApiResponse (legacy)
        if (result is ApiResponse apiResponse)
        {
            if (!apiResponse.Success)
            {
                throw new AppException(apiResponse.Message);
            }
        }
        
        return MapToDto(approval);
    }

    public async Task<ApprovalDto> RejectRequestAsync(string approvalId, string processedBy, string? reason = null)
    {
        var approval = await _approvalRepository.GetApprovalByIdAsync(approvalId) ?? throw new AppException("Approval not found", 404);
        if (approval.Status != "Pending")
        {
            throw new AppException("Only pending approvals can be processed", 400);
        }

        approval.Status = "Rejected";
        approval.ProcessedBy = processedBy;
        approval.ProcessedAt = DateTime.UtcNow;
        approval.Reason = reason;

        var result = await _approvalRepository.UpdateApprovalAsync(approval);
        
        // Check if the repository returns an ApiResponse (legacy)
        if (result is ApiResponse apiResponse)
        {
            if (!apiResponse.Success)
            {
                throw new AppException(apiResponse.Message);
            }
        }
        
        return MapToDto(approval);
    }

    public async Task<ApprovalDto> ProcessApprovalAsync(ProcessApprovalDto processDto, string processedBy)
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
            throw new AppException("Invalid action. Use 'approve' or 'reject'", 400);
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
