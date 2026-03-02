using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response.Remita;
using LendingSolution.Core.Models;

namespace LendingSolution.Application.Services.Interfaces;

public interface IRemitaService
{
    // ── Payday / Salary loan API ──────────────────────────────────────────────
    Task<RemitaSalaryHistoryResponseDto?> GetSalaryHistoryAsync(string accountNumber, string bankCode, string bvn, Guid borrowerApplicationId, string firstName = "", string lastName = "", string middleName = "", string? authorisationCode = null);
    Task<RemitaCreateMandateResponseDto?> CreateMandateAsync(string customerId, string phoneNumber, string accountNumber, string loanAmount, string collectionAmount, string dateOfDisbursement, string dateOfCollection, string totalCollectionAmount, string numberOfRepayments, string bankCode, string? authorisationCode = null);
    Task<RemitaStopMandateResponseDto?> StopMandateAsync(string customerId, string mandateReference, string? authorisationCode = null);
    Task<RemitaMandateHistoryResponseDto?> GetMandateHistoryAsync(string customerId, string mandateReference, string? authorisationCode = null);
    Task<BanksResponseDto?> GetBanksAsync();

    // ── Direct Debit mandate API (echannelsvc/echannel/mandate/) ─────────────
    Task<DirectDebitGenerateMandateResponseDto?> GenerateDirectDebitMandateAsync(DirectDebitGenerateMandateRequestDto request);
    Task<DirectDebitRequestAuthorizationResponseDto?> RequestMandateAuthorizationAsync(DirectDebitRequestAuthorizationDto request);
    Task<DirectDebitValidateAuthorizationResponseDto?> ValidateMandateAuthorizationAsync(DirectDebitValidateAuthorizationDto request);
    Task<DirectDebitStopMandateResponseDto?> StopDirectDebitMandateAsync(DirectDebitStopMandateRequestDto request);
}
