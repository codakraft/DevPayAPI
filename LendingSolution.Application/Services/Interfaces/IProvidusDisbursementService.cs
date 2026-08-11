using LendingSolution.Core.Dtos;

namespace LendingSolution.Application.Services.Interfaces;

/// <summary>
/// Interface for Providus Bank disbursement operations
/// </summary>
public interface IProvidusDisbursementService
{
    /// <summary>
    /// Transfer funds from the company's Providus account to a beneficiary account
    /// </summary>
    /// <param name="request">The disbursement request details</param>
    /// <returns>The result of the disbursement operation</returns>
    Task<DisbursementResultDto> TransferFundsAsync(ProvidusDisbursementInternalRequestDto request);
    
    /// <summary>
    /// Transfer funds using raw Providus API parameters
    /// </summary>
    /// <param name="creditAccount">Destination account number</param>
    /// <param name="amount">Amount to transfer</param>
    /// <param name="narration">Transaction narration</param>
    /// <param name="transactionReference">Optional custom reference (auto-generated if not provided)</param>
    /// <returns>The Providus API response</returns>
    Task<ProvidusFundTransferResponseDto> TransferFundsRawAsync(
        string creditAccount, 
        decimal amount, 
        string narration,
        string? transactionReference = null);
    
    /// <summary>
    /// Resolve the account name held at the beneficiary bank, via NIP name enquiry.
    /// </summary>
    /// <param name="accountNumber">The beneficiary account number</param>
    /// <param name="bankCode">The beneficiary's CBN bank code, as stored on the borrower record</param>
    /// <returns>The name enquiry result; check IsSuccessful before using AccountName</returns>
    Task<ProvidusNipAccountEnquiryResponseDto> NameEnquiryAsync(string accountNumber, string bankCode);

    /// <summary>
    /// Verify the status of a previous transaction
    /// </summary>
    /// <param name="transactionReference">The transaction reference to verify</param>
    /// <returns>The verification result</returns>
    Task<DisbursementResultDto> VerifyTransactionAsync(string transactionReference);
    
    /// <summary>
    /// Check if the service is currently in mock mode
    /// </summary>
    bool IsMockMode { get; }
    
    /// <summary>
    /// Generate a unique transaction reference
    /// </summary>
    /// <returns>A unique transaction reference string</returns>
    string GenerateTransactionReference();
}
