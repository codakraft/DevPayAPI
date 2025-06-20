using LendingSolution.Core.Dtos.Response.Remita;

namespace LendingSolution.Application.Services.Interfaces;

public interface IRemitaService
{
    string GetUsername();
    string GetPassword();
    Task<SalaryHistoryResponse> GetSalaryHistoryAsync(object requestBody);
}