using LendingSolution.Core.Models.Response;
using System.Threading.Tasks;

namespace LendingSolution.Application.Services.Interfaces;

public interface IProfileService
{
    Task<ApiResponse> GetUserProfile();
}
