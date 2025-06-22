using System.Threading.Tasks;
using System.Collections.Generic;
using LendingSolution.Application.Services.Interfaces;

namespace LendingSolution.Application.Services.Implementations
{
    public class SupportToolsService : ISupportToolsService
    {
        public async Task<object> GetAccountDetailsAsync(string bankCode, string accountNumber)
        {
            return await Task.FromResult(new { });
        }

        public async Task<object> GetBvnDetailsAsync(string bvn)
        {
            // TODO: Validate input and call external API
            return await Task.FromResult(new { });
        }

        public async Task<IEnumerable<object>> GetBanksAndCodesAsync()
        {
            // TODO: Call external API
            return await Task.FromResult(new List<object>());
        }

        public async Task<IEnumerable<object>> GetRemitaBanksAndCodesAsync()
        {
            // TODO: Call external API
            return await Task.FromResult(new List<object>());
        }
    }
}
