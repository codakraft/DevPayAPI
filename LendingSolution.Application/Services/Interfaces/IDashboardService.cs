using System.Threading.Tasks;

namespace LendingSolution.Application.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDataDto> GetDashboardDataAsync();
    }

}
