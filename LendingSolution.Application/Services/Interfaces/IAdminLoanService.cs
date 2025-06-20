using LendingSolution.Core.Dtos; // Add this or the correct namespace for LoanRequestDto

namespace LendingSolution.Application.Services.Interfaces;

public interface IAdminLoanService
{
    Task<IEnumerable<LoanRequestDto>> GetLoanRequestsAsync(LoanRequestFilterDto filter);
    Task<LoanRequestDto> GetLoanRequestByIdAsync(string id);
    Task<bool> ApproveLoanRequestAsync(Guid id);
    Task<bool> RejectLoanRequestAsync(Guid id, string message);
}


