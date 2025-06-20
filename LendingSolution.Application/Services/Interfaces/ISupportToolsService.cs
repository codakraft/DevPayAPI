using System.Threading.Tasks;
using System.Collections.Generic;

namespace LendingSolution.Application.Services.Interfaces
{
    public interface ISupportToolsService
    {
        Task<object> GetAccountDetailsAsync(string bankCode, string accountNumber);
        Task<object> GetBvnDetailsAsync(string bvn);
        Task<IEnumerable<object>> GetBanksAndCodesAsync();
        Task<IEnumerable<object>> GetRemitaBanksAndCodesAsync();
    }
}
