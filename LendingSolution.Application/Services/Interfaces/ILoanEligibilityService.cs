using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LendingSolution.Application.Services.Interfaces
{
    public interface ILoanEligibilityService
    {
        Task<IEnumerable<LoanEligibilityDto>> GetLoanEligibilityListAsync(LoanEligibilityFilterDto filter);
    }

}
