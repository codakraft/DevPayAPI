using Microsoft.EntityFrameworkCore;
using LendingSolution.Core.Models;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Infrastructure.Data;

namespace LendingSolution.Application.Repositories.Implementations;

public class ApprovalRepository(ApplicationDbContext context) : IApprovalRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<Approval>> GetAllApprovalsAsync()
    {
        return await _context.Approvals
            .Include(a => a.RequestedByUser)
            .Include(a => a.ProcessedByUser)
            .Include(a => a.Company)
            .OrderByDescending(a => a.RequestedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Approval>> GetPendingApprovalsAsync()
    {
        return await _context.Approvals
            .Include(a => a.RequestedByUser)
            .Include(a => a.Company)
            .Where(a => a.Status == "Pending")
            .OrderBy(a => a.RequestedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Approval>> GetApprovalsByStatusAsync(string status)
    {
        return await _context.Approvals
            .Include(a => a.RequestedByUser)
            .Include(a => a.ProcessedByUser)
            .Include(a => a.Company)
            .Where(a => a.Status == status)
            .OrderByDescending(a => a.RequestedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Approval>> GetApprovalsByTypeAsync(string approvalType)
    {
        return await _context.Approvals
            .Include(a => a.RequestedByUser)
            .Include(a => a.ProcessedByUser)
            .Include(a => a.Company)
            .Where(a => a.ApprovalType == approvalType)
            .OrderByDescending(a => a.RequestedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Approval>> GetApprovalsByCompanyAsync(string companyId)
    {
        if (!Guid.TryParse(companyId, out var companyGuid))
        {
            return new List<Approval>();
        }

        return await _context.Approvals
            .Include(a => a.RequestedByUser)
            .Include(a => a.ProcessedByUser)
            .Include(a => a.Company)
            .Where(a => a.CompanyId == companyGuid)
            .OrderByDescending(a => a.RequestedAt)
            .ToListAsync();
    }

    public async Task<Approval?> GetApprovalByIdAsync(string id)
    {
        return await _context.Approvals
            .Include(a => a.RequestedByUser)
            .Include(a => a.ProcessedByUser)
            .Include(a => a.Company)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<ApiResponse> CreateApprovalAsync(Approval approval)
    {
        try
        {
            approval.RequestedAt = DateTime.UtcNow;
            approval.Status = "Pending";
            
            await _context.Approvals.AddAsync(approval);
            await _context.SaveChangesAsync();
            
            return ApiResponse.Ok("Approval request created successfully", approval);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to create approval: {ex.Message}");
        }
    }

    public async Task<ApiResponse> UpdateApprovalAsync(Approval approval)
    {
        try
        {
            _context.Approvals.Update(approval);
            await _context.SaveChangesAsync();
            
            return ApiResponse.Ok("Approval updated successfully", approval);
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to update approval: {ex.Message}");
        }
    }

    public async Task<ApiResponse> DeleteApprovalAsync(string id)
    {
        try
        {
            var approval = await GetApprovalByIdAsync(id);
            if (approval == null)
            {
                return ApiResponse.Fail("Approval not found");
            }

            _context.Approvals.Remove(approval);
            await _context.SaveChangesAsync();
            
            return ApiResponse.Ok("Approval deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse.Fail($"Failed to delete approval: {ex.Message}");
        }
    }

    public async Task<bool> ApprovalExistsAsync(string id)
    {
        return await _context.Approvals
            .AnyAsync(a => a.Id == id);
    }
}
