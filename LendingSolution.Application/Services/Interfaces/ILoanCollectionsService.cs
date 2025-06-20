using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LendingSolution.Application.Services.Interfaces
{
    public interface ILoanCollectionsService
    {
        Task<IEnumerable<LoanRequestDto>> GetLoanRequestsAsync(LoanRequestFilterDto filter);
        Task<LoanRequestDto> GetLoanRequestByIdAsync(string id);
        Task<bool> ApproveLoanRequestAsync(string id);
        Task<bool> RejectLoanRequestAsync(string id, string message = null);
        Task<IEnumerable<UnpaidLoanDto>> GetUnpaidLoansAsync();
        Task<IEnumerable<OngoingCollectionDto>> GetOngoingCollectionsAsync();
        Task<IEnumerable<UpcomingCollectionDto>> GetUpcomingCollectionsAsync(DateTime? dueDate = null);
        Task<IEnumerable<SuccessfulRepaymentDto>> GetSuccessfulRepaymentsAsync(DateTime? transactionDate = null);
    }

}
