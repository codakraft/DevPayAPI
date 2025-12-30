using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response.Remita;
using LendingSolution.Core.Models;

namespace LendingSolution.Application.Services.Interfaces;

public interface IRemitaService
{
    Task<RemitaSalaryHistoryResponseDto?> GetSalaryHistoryAsync(string accountNumber, string bankCode, string bvn, string email, string firstName = "", string lastName = "", string middleName = "", string? authorisationCode = null);
    Task<RemitaCreateMandateResponseDto?> CreateMandateAsync(string customerId, string phoneNumber, string accountNumber, string loanAmount, string collectionAmount, string dateOfDisbursement, string dateOfCollection, string totalCollectionAmount, string numberOfRepayments, string bankCode, string? authorisationCode = null);
    Task<RemitaStopMandateResponseDto?> StopMandateAsync(string customerId, string mandateReference, string? authorisationCode = null);
    Task<RemitaMandateHistoryResponseDto?> GetMandateHistoryAsync(string customerId, string mandateReference, string? authorisationCode = null);
    Task<BanksResponseDto?> GetBanksAsync();
    Task<RemitaLoanCollectionNotification?> ProcessLoanCollectionNotificationAsync(RemitaLoanCollectionNotificationDto notification);
}
