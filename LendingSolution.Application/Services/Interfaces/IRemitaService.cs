using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response.Remita;

namespace LendingSolution.Application.Services.Interfaces;

public interface IRemitaService
{
    // Existing methods
    Task<object?> GetSalaryHistory(Guid loanId, ReviewHistoryRequestDto body);
    Task<MandateResponse?> GenerateMandate(Guid LoanId, SubmitRequestDto body);
    Task<InitiateMandateOtpResponseDto?> Initiate(Guid loanId);
    Task<ValidateMandateOtpResponseDto?> ValidateMandate(Guid loanId, ValidateMandateOtpRequestDto body);
    Task<DebitInstructionResponseDto?> DebitInstruction(Guid loanId, DebitInstructionRequestDto body);

    // New methods for RemitaController
    Task<SalaryHistoryResponse?> GetSalaryHistoryByBvnAsync(SalaryHistoryRequestDto request);
    Task<AccountVerificationResponseDto?> VerifyAccountAsync(AccountVerificationRequestDto request);
    Task<CreateMandateResponseDto?> CreateLoanMandateAsync(Guid loanId, CreateMandateRequestDto request, string? userId);
    Task<DisbursementResponseDto?> ProcessLoanDisbursementAsync(Guid loanId, RemitaDisbursementRequestDto request, string? userId);
    Task<RepaymentCollectionResponseDto?> CollectRepaymentAsync(Guid loanId, RepaymentCollectionRequestDto request, string? userId);
    Task<TransactionStatusResponseDto?> GetTransactionStatusAsync(string transactionRef);
    Task<BanksResponseDto?> GetBanksAsync();
    Task<bool> ProcessWebhookNotificationAsync(RemitaWebhookNotificationDto notification);
}
