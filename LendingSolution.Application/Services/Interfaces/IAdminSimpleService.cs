using System.Threading.Tasks;

namespace LendingSolution.Application.Services.Interfaces
{
    public interface IAdminSimpleService
    {
        Task<UserSimpleAdminDto> GetUseAsync();
    }

}
