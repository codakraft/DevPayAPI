using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LendingSolution.Application.Services.Implementations;

/// <summary>
/// Service for handling Providus Bank fund transfers and disbursements
/// </summary>
public class ProvidusDisbursementService : IProvidusDisbursementService
{
    private readonly ProvidusSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ProvidusDisbursementService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ProvidusDisbursementService(
        IOptions<ProvidusSettings> settings,
        IHttpClientFactory httpClientFactory,
        ILogger<ProvidusDisbursementService> logger)
    {
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public bool IsMockMode => _settings.UseMockMode;

    /// <inheritdoc />
    public string GenerateTransactionReference()
    {
        // Format: YYYYMMDDHHMMSS + random 6 digits
        return $"{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(100000, 999999)}";
    }

    /// <inheritdoc />
    public async Task<DisbursementResultDto> TransferFundsAsync(ProvidusDisbursementInternalRequestDto request)
    {
        var transactionReference = GenerateTransactionReference();
        var narration = request.Narration ?? $"DevPay Loan Disbursement - {request.LoanId}";

        _logger.LogInformation(
            "Initiating fund transfer for Loan {LoanId}. Amount: {Amount}, Account: {Account}, BankCode: {BankCode}, Reference: {Reference}, MockMode: {MockMode}",
            request.LoanId, request.Amount, request.DestinationAccountNumber, request.DestinationBankCode, transactionReference, IsMockMode);

        try
        {
            var response = await TransferFundsRawAsync(
                request.DestinationAccountNumber,
                request.Amount,
                narration,
                transactionReference);

            var result = new DisbursementResultDto
            {
                IsSuccessful = response.IsSuccessful,
                LoanId = request.LoanId,
                Amount = request.Amount,
                DisbursementReference = transactionReference,
                ProviderReference = response.TransactionReference,
                Message = response.ResponseMessage,
                ResponseCode = response.ResponseCode,
                DisbursedAt = DateTime.UtcNow,
                IsMockTransaction = IsMockMode
            };

            if (response.IsSuccessful)
            {
                _logger.LogInformation(
                    "Fund transfer successful for Loan {LoanId}. Reference: {Reference}, Provider Ref: {ProviderRef}",
                    request.LoanId, transactionReference, response.TransactionReference);
            }
            else
            {
                _logger.LogWarning(
                    "Fund transfer failed for Loan {LoanId}. Reference: {Reference}, Code: {Code}, Message: {Message}",
                    request.LoanId, transactionReference, response.ResponseCode, response.ResponseMessage);
                result.ErrorDetails = $"Provider response: {response.ResponseCode} - {response.ResponseMessage}";
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Exception during fund transfer for Loan {LoanId}. Reference: {Reference}",
                request.LoanId, transactionReference);

            return new DisbursementResultDto
            {
                IsSuccessful = false,
                LoanId = request.LoanId,
                Amount = request.Amount,
                DisbursementReference = transactionReference,
                Message = "Fund transfer failed due to an error",
                ResponseCode = "99",
                DisbursedAt = DateTime.UtcNow,
                IsMockTransaction = IsMockMode,
                ErrorDetails = ex.Message
            };
        }
    }

    /// <inheritdoc />
    public async Task<ProvidusFundTransferResponseDto> TransferFundsRawAsync(
        string creditAccount,
        decimal amount,
        string narration,
        string? transactionReference = null)
    {
        var reference = transactionReference ?? GenerateTransactionReference();

        // If in mock mode, return a simulated successful response
        if (IsMockMode)
        {
            _logger.LogInformation(
                "MOCK MODE: Simulating fund transfer. Credit: {CreditAccount}, Amount: {Amount}, Reference: {Reference}",
                creditAccount, amount, reference);

            await Task.Delay(500); // Simulate network delay

            return GenerateMockResponse(amount, reference);
        }

        // Build the request
        var request = new ProvidusFundTransferRequestDto
        {
            CreditAccount = creditAccount,
            DebitAccount = _settings.DebitAccount,
            TransactionAmount = amount.ToString("F2"),
            CurrencyCode = _settings.CurrencyCode,
            Narration = narration,
            TransactionReference = reference,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        return await CallProvidusApiAsync(request);
    }

    /// <inheritdoc />
    public async Task<DisbursementResultDto> VerifyTransactionAsync(string transactionReference)
    {
        // Note: This is a placeholder. Providus may have a separate endpoint for transaction verification.
        // For now, we return a basic response indicating the transaction reference was logged.
        
        _logger.LogInformation("Transaction verification requested for reference: {Reference}", transactionReference);

        if (IsMockMode)
        {
            await Task.Delay(200);
            return new DisbursementResultDto
            {
                IsSuccessful = true,
                DisbursementReference = transactionReference,
                Message = "MOCK: Transaction verification successful",
                ResponseCode = "00",
                DisbursedAt = DateTime.UtcNow,
                IsMockTransaction = true
            };
        }

        // TODO: Implement actual transaction verification when Providus provides the endpoint
        return new DisbursementResultDto
        {
            IsSuccessful = false,
            DisbursementReference = transactionReference,
            Message = "Transaction verification not yet implemented",
            ResponseCode = "99",
            DisbursedAt = DateTime.UtcNow,
            IsMockTransaction = false
        };
    }

    #region Private Methods

    private async Task<ProvidusFundTransferResponseDto> CallProvidusApiAsync(ProvidusFundTransferRequestDto request)
    {
        var client = _httpClientFactory.CreateClient("Providus");
        client.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

        var url = $"{_settings.BaseUrl.TrimEnd('/')}{_settings.FundTransferEndpoint}";

        _logger.LogInformation(
            "Calling Providus API. URL: {Url}, Reference: {Reference}, Amount: {Amount}",
            url, request.TransactionReference, request.TransactionAmount);

        try
        {
            var jsonContent = JsonSerializer.Serialize(request, JsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogInformation("Providus API Request: {Request}",
                JsonSerializer.Serialize(new
                {
                    request.CreditAccount,
                    request.DebitAccount,
                    request.TransactionAmount,
                    request.CurrencyCode,
                    request.Narration,
                    request.TransactionReference,
                    UserName = "***",
                    Password = "***"
                }));

            var response = await client.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation(
                "Providus API Response. StatusCode: {StatusCode}, Reference: {Reference}",
                response.StatusCode, request.TransactionReference);
            _logger.LogInformation("Providus API Response Body: {Response}", responseContent);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ProvidusFundTransferResponseDto>(responseContent, JsonOptions);
                return result ?? new ProvidusFundTransferResponseDto
                {
                    ResponseCode = "99",
                    ResponseMessage = "Failed to parse response"
                };
            }

            return new ProvidusFundTransferResponseDto
            {
                ResponseCode = ((int)response.StatusCode).ToString(),
                ResponseMessage = $"HTTP Error: {response.StatusCode} - {responseContent}"
            };
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Providus API call timed out. Reference: {Reference}", request.TransactionReference);
            return new ProvidusFundTransferResponseDto
            {
                ResponseCode = "TIMEOUT",
                ResponseMessage = "Request timed out"
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Providus API. Reference: {Reference}", request.TransactionReference);
            return new ProvidusFundTransferResponseDto
            {
                ResponseCode = "HTTP_ERROR",
                ResponseMessage = $"Connection error: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Providus API. Reference: {Reference}", request.TransactionReference);
            return new ProvidusFundTransferResponseDto
            {
                ResponseCode = "ERROR",
                ResponseMessage = $"Unexpected error: {ex.Message}"
            };
        }
    }

    private ProvidusFundTransferResponseDto GenerateMockResponse(decimal amount, string reference)
    {
        // Simulate different scenarios based on amount for testing
        // Amounts ending in .99 will simulate failures
        if (amount.ToString("F2").EndsWith(".99"))
        {
            _logger.LogInformation("MOCK MODE: Simulating failed transaction for testing");
            return new ProvidusFundTransferResponseDto
            {
                Amount = amount.ToString("F2"),
                TransactionReference = reference,
                Currency = _settings.CurrencyCode,
                ResponseCode = "51",
                ResponseMessage = "MOCK: Insufficient funds"
            };
        }

        // Amounts ending in .98 will simulate timeout
        if (amount.ToString("F2").EndsWith(".98"))
        {
            _logger.LogInformation("MOCK MODE: Simulating timeout for testing");
            return new ProvidusFundTransferResponseDto
            {
                Amount = amount.ToString("F2"),
                TransactionReference = reference,
                Currency = _settings.CurrencyCode,
                ResponseCode = "TIMEOUT",
                ResponseMessage = "MOCK: Request timed out"
            };
        }

        // Default: successful transaction
        return new ProvidusFundTransferResponseDto
        {
            Amount = amount.ToString("F2"),
            TransactionReference = $"PROV{reference}",
            Currency = _settings.CurrencyCode,
            ResponseCode = "00",
            ResponseMessage = "MOCK: OPERATION SUCCESSFUL"
        };
    }

    #endregion
}
