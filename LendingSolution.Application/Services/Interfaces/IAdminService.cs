using System.Collections.Generic;
using System.Threading.Tasks;

namespace LendingSolution.Application.Services.Interfaces
{
    public interface IAdminService
    {
        Task<IEnumerable<UserAdminDto>> GetUsersAsync(UserFilterDto filter);
        Task<bool> DeactivateUserAsync(string userId);
        Task<bool> ActivateUserAsync(string userId);
    }

}
