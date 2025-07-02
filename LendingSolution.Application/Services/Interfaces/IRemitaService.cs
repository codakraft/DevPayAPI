using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response.Remita;

namespace LendingSolution.Application.Services.Interfaces;

public interface IRemitaService
{
    Task<object?> GetSalaryHistory(Guid loanId, ReviewHistoryRequestDto body);
    Task<MandateResponse?> GenerateMandate(Guid LoanId, SubmitRequestDto body);
    Task<InitiateMandateOtpResponseDto?> Initiate(Guid loanId);
    Task<ValidateMandateOtpResponseDto?> ValidateMandate(Guid loanId, ValidateMandateOtpRequestDto body);
    Task<DebitInstructionResponseDto?> DebitInstruction(Guid loanId, DebitInstructionRequestDto body);
}
