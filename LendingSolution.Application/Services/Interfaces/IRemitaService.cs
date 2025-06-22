using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response.Remita;

namespace LendingSolution.Application.Services.Interfaces;

public interface IRemitaService
{
    Task<SalaryHistoryResponse?> GetSalaryHistory(object requestBody);
    Task<MandateResponse?> GenerateMandate(Guid LoanId, SubmitRequestDto request);
}
