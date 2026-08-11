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
/// <remarks>
/// Transfers are routed by beneficiary bank. Providus-to-Providus transfers use the
/// intra-bank posting endpoint, which carries no NIBSS fee; everything else goes over
/// NIP. Sending an inter-bank beneficiary to the intra-bank endpoint is what produces
/// "7701 CREDIT ACCOUNT IS INVALID" — that endpoint only sees Providus's own ledger.
/// </remarks>
public class ProvidusDisbursementService : IProvidusDisbursementService
{
    private readonly ProvidusSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IBankCodeResolver _bankCodeResolver;
    private readonly ILogger<ProvidusDisbursementService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ProvidusDisbursementService(
        IOptions<ProvidusSettings> settings,
        IHttpClientFactory httpClientFactory,
        IBankCodeResolver bankCodeResolver,
        ILogger<ProvidusDisbursementService> logger)
    {
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
        _bankCodeResolver = bankCodeResolver;
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
        // Reuse the caller's reference when supplied. Providus deduplicates on this value,
        // so minting a fresh one per attempt would defeat the guard against double payment.
        var transactionReference = string.IsNullOrWhiteSpace(request.TransactionReference)
            ? GenerateTransactionReference()
            : request.TransactionReference.Trim();

        var narration = request.Narration ?? $"DevPay Loan Disbursement - {request.LoanId}";

        _logger.LogInformation(
            "Initiating fund transfer for Loan {LoanId}. Amount: {Amount}, Account: {Account}, BankCode: {BankCode}, Reference: {Reference}, MockMode: {MockMode}",
            request.LoanId, request.Amount, request.DestinationAccountNumber, request.DestinationBankCode,
            transactionReference, IsMockMode);

        try
        {
            var nipBankCode = await _bankCodeResolver.ResolveNipCodeAsync(request.DestinationBankCode);

            if (nipBankCode == null)
            {
                // Sending an unresolved CBN code as beneficiaryBank would be rejected by
                // Providus anyway, but failing here gives a diagnosable message instead.
                if (IsMockMode)
                {
                    _logger.LogWarning(
                        "MOCK MODE: bank code {BankCode} did not resolve to a NIP code. A live transfer would fail here.",
                        request.DestinationBankCode);

                    return BuildResult(request, transactionReference, GenerateMockResponse(request.Amount, transactionReference));
                }

                _logger.LogError(
                    "Cannot disburse Loan {LoanId}: bank code {BankCode} did not resolve to a NIP code",
                    request.LoanId, request.DestinationBankCode);

                return new DisbursementResultDto
                {
                    IsSuccessful = false,
                    LoanId = request.LoanId,
                    Amount = request.Amount,
                    DisbursementReference = transactionReference,
                    Message = "Beneficiary bank is not recognised",
                    ResponseCode = ProvidusResponseCodes.UnresolvedBankCode,
                    DisbursedAt = DateTime.UtcNow,
                    IsMockTransaction = false,
                    ErrorDetails = $"Bank code '{request.DestinationBankCode}' could not be mapped to a NIP institution code."
                };
            }

            var isIntraBank = string.Equals(nipBankCode, _settings.ProvidusNipBankCode, StringComparison.OrdinalIgnoreCase);

            _logger.LogInformation(
                "Loan {LoanId} routed as {Route}. BankCode {BankCode} -> NIP {NipCode}",
                request.LoanId, isIntraBank ? "INTRA-BANK" : "NIP", request.DestinationBankCode, nipBankCode);

            return isIntraBank
                ? await TransferIntraBankAsync(request, transactionReference, narration)
                : await TransferViaNipAsync(request, nipBankCode, transactionReference, narration);
        }
        catch (Exception ex)
        {
            // The transfer may or may not have reached Providus. Treat as indeterminate so
            // the caller requeries rather than retrying with a new reference.
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
                ResponseCode = ProvidusResponseCodes.Error,
                DisbursedAt = DateTime.UtcNow,
                IsMockTransaction = IsMockMode,
                ErrorDetails = ex.Message,
                IsIndeterminate = true
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

        var call = await PostAsync<ProvidusFundTransferResponseDto>(
            _settings.FundTransferEndpoint, request, reference);

        return call.Response ?? new ProvidusFundTransferResponseDto
        {
            ResponseCode = call.ErrorCode,
            ResponseMessage = call.ErrorMessage
        };
    }

    /// <inheritdoc />
    public async Task<ProvidusNipAccountEnquiryResponseDto> NameEnquiryAsync(string accountNumber, string bankCode)
    {
        var nipBankCode = await _bankCodeResolver.ResolveNipCodeAsync(bankCode);

        if (nipBankCode == null)
        {
            return new ProvidusNipAccountEnquiryResponseDto
            {
                AccountNumber = accountNumber,
                ResponseCode = ProvidusResponseCodes.UnresolvedBankCode,
                ResponseMessage = $"Bank code '{bankCode}' could not be mapped to a NIP institution code."
            };
        }

        return await NameEnquiryByNipCodeAsync(accountNumber, nipBankCode);
    }

    /// <inheritdoc />
    public async Task<DisbursementResultDto> VerifyTransactionAsync(string transactionReference)
    {
        _logger.LogInformation("Transaction verification requested for reference: {Reference}", transactionReference);

        if (IsMockMode)
        {
            await Task.Delay(200);
            return new DisbursementResultDto
            {
                IsSuccessful = true,
                DisbursementReference = transactionReference,
                Message = "MOCK: Transaction verification successful",
                ResponseCode = ProvidusResponseCodes.Success,
                DisbursedAt = DateTime.UtcNow,
                IsMockTransaction = true
            };
        }

        var request = new ProvidusNipTransactionStatusRequestDto
        {
            TransactionReference = transactionReference,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        var call = await PostAsync<ProvidusNipTransactionStatusResponseDto>(
            _settings.NipTransactionStatusEndpoint, request, transactionReference);

        if (call.Response == null)
        {
            // The requery itself failed, which says nothing about the transfer.
            return new DisbursementResultDto
            {
                IsSuccessful = false,
                DisbursementReference = transactionReference,
                Message = "Unable to verify transaction status",
                ResponseCode = call.ErrorCode,
                DisbursedAt = DateTime.UtcNow,
                ErrorDetails = call.ErrorMessage,
                IsIndeterminate = true
            };
        }

        var status = call.Response;

        // Providus omits the amount when it has no record of the reference; 0 is the
        // correct reading there, and callers substitute the expected amount if they have it.
        var amount = decimal.TryParse(status.Amount, out var parsedAmount) ? parsedAmount : 0m;

        return new DisbursementResultDto
        {
            IsSuccessful = status.IsSuccessful,
            Amount = amount,
            DisbursementReference = transactionReference,
            ProviderReference = status.TransactionReference,
            Message = status.ResponseMessage,
            ResponseCode = status.ResponseCode,
            DisbursedAt = DateTime.UtcNow,
            IsMockTransaction = false,
            // "Not found" is a definitive answer: no funds moved against this reference.
            IsIndeterminate = !status.IsSuccessful && !status.IsNotFound,
            ErrorDetails = status.IsSuccessful
                ? null
                : $"Provider response: {status.ResponseCode} - {status.ResponseMessage}"
        };
    }

    #region Private Methods

    private async Task<DisbursementResultDto> TransferIntraBankAsync(
        ProvidusDisbursementInternalRequestDto request,
        string transactionReference,
        string narration)
    {
        var response = await TransferFundsRawAsync(
            request.DestinationAccountNumber,
            request.Amount,
            narration,
            transactionReference);

        return BuildResult(request, transactionReference, response);
    }

    private async Task<DisbursementResultDto> TransferViaNipAsync(
        ProvidusDisbursementInternalRequestDto request,
        string nipBankCode,
        string transactionReference,
        string narration)
    {
        if (IsMockMode)
        {
            _logger.LogInformation(
                "MOCK MODE: Simulating NIP transfer. Account: {Account}, NIP bank: {NipBank}, Amount: {Amount}, Reference: {Reference}",
                request.DestinationAccountNumber, nipBankCode, request.Amount, transactionReference);

            await Task.Delay(500);

            return BuildResult(request, transactionReference, GenerateMockResponse(request.Amount, transactionReference));
        }

        // NIPFundTransfer requires beneficiaryAccountName, and it must be the name the
        // beneficiary bank holds — not the name typed during onboarding. Name enquiry is
        // also a cheap way to catch a bad account before any money moves.
        var enquiry = await NameEnquiryByNipCodeAsync(request.DestinationAccountNumber, nipBankCode);

        if (!enquiry.IsSuccessful || string.IsNullOrWhiteSpace(enquiry.AccountName))
        {
            _logger.LogWarning(
                "Name enquiry failed for Loan {LoanId}. Account: {Account}, NIP bank: {NipBank}, Code: {Code}, Message: {Message}",
                request.LoanId, request.DestinationAccountNumber, nipBankCode,
                enquiry.ResponseCode, enquiry.ResponseMessage);

            return new DisbursementResultDto
            {
                IsSuccessful = false,
                LoanId = request.LoanId,
                Amount = request.Amount,
                DisbursementReference = transactionReference,
                Message = "Beneficiary account could not be verified",
                ResponseCode = string.IsNullOrWhiteSpace(enquiry.ResponseCode)
                    ? ProvidusResponseCodes.NameEnquiryFailed
                    : enquiry.ResponseCode,
                DisbursedAt = DateTime.UtcNow,
                IsMockTransaction = false,
                ErrorDetails = $"Name enquiry: {enquiry.ResponseCode} - {enquiry.ResponseMessage}"
            };
        }

        _logger.LogInformation(
            "Name enquiry resolved account {Account} at NIP bank {NipBank} to '{AccountName}' for Loan {LoanId}",
            request.DestinationAccountNumber, nipBankCode, enquiry.AccountName, request.LoanId);

        var payload = new ProvidusNipFundTransferRequestDto
        {
            BeneficiaryAccountName = enquiry.AccountName,
            BeneficiaryAccountNumber = request.DestinationAccountNumber,
            BeneficiaryBank = nipBankCode,
            TransactionAmount = request.Amount.ToString("F2"),
            CurrencyCode = _settings.CurrencyCode,
            Narration = narration,
            SourceAccountName = _settings.SourceAccountName,
            TransactionReference = transactionReference,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        var call = await PostAsync<ProvidusNipFundTransferResponseDto>(
            _settings.NipFundTransferEndpoint, payload, transactionReference);

        if (call.Response == null)
        {
            // No response means we cannot know whether the debit landed.
            _logger.LogError(
                "NIP transfer outcome unknown for Loan {LoanId}. Reference: {Reference}, Error: {Error}",
                request.LoanId, transactionReference, call.ErrorMessage);

            return new DisbursementResultDto
            {
                IsSuccessful = false,
                LoanId = request.LoanId,
                Amount = request.Amount,
                DisbursementReference = transactionReference,
                Message = "Transfer outcome could not be confirmed",
                ResponseCode = call.ErrorCode,
                DisbursedAt = DateTime.UtcNow,
                IsMockTransaction = false,
                ErrorDetails = call.ErrorMessage,
                ResolvedBeneficiaryName = enquiry.AccountName,
                IsIndeterminate = true
            };
        }

        var nip = call.Response;

        // Providus already has this reference, so this attempt did not create a second
        // transfer. Ask what happened to the original rather than reporting a failure.
        if (nip.IsDuplicateReference)
        {
            _logger.LogWarning(
                "Providus reports reference {Reference} already exists for Loan {LoanId}; requerying original transfer",
                transactionReference, request.LoanId);

            var verified = await VerifyTransactionAsync(transactionReference);
            verified.LoanId = request.LoanId;
            verified.ResolvedBeneficiaryName = enquiry.AccountName;

            if (verified.Amount == 0)
            {
                verified.Amount = request.Amount;
            }

            return verified;
        }

        if (nip.IsSuccessful)
        {
            _logger.LogInformation(
                "NIP transfer successful for Loan {LoanId}. Reference: {Reference}, SessionId: {SessionId}",
                request.LoanId, transactionReference, nip.SessionId);
        }
        else
        {
            _logger.LogWarning(
                "NIP transfer failed for Loan {LoanId}. Reference: {Reference}, Code: {Code}, Message: {Message}",
                request.LoanId, transactionReference, nip.ResponseCode, nip.ResponseMessage);
        }

        return new DisbursementResultDto
        {
            IsSuccessful = nip.IsSuccessful,
            LoanId = request.LoanId,
            Amount = request.Amount,
            DisbursementReference = transactionReference,
            ProviderReference = string.IsNullOrWhiteSpace(nip.TransactionReference)
                ? transactionReference
                : nip.TransactionReference,
            Message = nip.ResponseMessage,
            ResponseCode = nip.ResponseCode,
            DisbursedAt = DateTime.UtcNow,
            IsMockTransaction = false,
            SessionId = nip.SessionId,
            ResolvedBeneficiaryName = enquiry.AccountName,
            ErrorDetails = nip.IsSuccessful
                ? null
                : $"Provider response: {nip.ResponseCode} - {nip.ResponseMessage}"
        };
    }

    private async Task<ProvidusNipAccountEnquiryResponseDto> NameEnquiryByNipCodeAsync(
        string accountNumber, string nipBankCode)
    {
        if (IsMockMode)
        {
            await Task.Delay(200);
            return new ProvidusNipAccountEnquiryResponseDto
            {
                AccountName = "MOCK BENEFICIARY",
                AccountNumber = accountNumber,
                BankCode = nipBankCode,
                ResponseCode = ProvidusResponseCodes.Success,
                ResponseMessage = "MOCK: Name enquiry successful"
            };
        }

        var payload = new ProvidusNipAccountEnquiryRequestDto
        {
            AccountNumber = accountNumber,
            BeneficiaryBank = nipBankCode,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        var call = await PostAsync<ProvidusNipAccountEnquiryResponseDto>(
            _settings.NipAccountEnquiryEndpoint, payload, accountNumber);

        return call.Response ?? new ProvidusNipAccountEnquiryResponseDto
        {
            AccountNumber = accountNumber,
            BankCode = nipBankCode,
            ResponseCode = call.ErrorCode,
            ResponseMessage = call.ErrorMessage
        };
    }

    private DisbursementResultDto BuildResult(
        ProvidusDisbursementInternalRequestDto request,
        string transactionReference,
        ProvidusFundTransferResponseDto response)
    {
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
        }

        return new DisbursementResultDto
        {
            IsSuccessful = response.IsSuccessful,
            LoanId = request.LoanId,
            Amount = request.Amount,
            DisbursementReference = transactionReference,
            ProviderReference = response.TransactionReference,
            Message = response.ResponseMessage,
            ResponseCode = response.ResponseCode,
            DisbursedAt = DateTime.UtcNow,
            IsMockTransaction = IsMockMode,
            IsIndeterminate = response.ResponseCode == ProvidusResponseCodes.Timeout,
            ErrorDetails = response.IsSuccessful
                ? null
                : $"Provider response: {response.ResponseCode} - {response.ResponseMessage}"
        };
    }

    /// <summary>
    /// Result of a single Providus API call. Response is null whenever no parseable
    /// response body was obtained, in which case ErrorCode/ErrorMessage explain why.
    /// </summary>
    private sealed record ProvidusApiCall<T>(T? Response, string ErrorCode, string ErrorMessage);

    private async Task<ProvidusApiCall<TResponse>> PostAsync<TResponse>(
        string endpointPath,
        object payload,
        string reference)
        where TResponse : class
    {
        var client = _httpClientFactory.CreateClient("Providus");
        client.Timeout = TimeSpan.FromSeconds(_settings.TimeoutSeconds);

        var url = $"{_settings.BaseUrl.TrimEnd('/')}{endpointPath}";

        _logger.LogInformation(
            "Calling Providus API. URL: {Url}, Reference: {Reference}", url, reference);

        try
        {
            var jsonContent = JsonSerializer.Serialize(payload, payload.GetType(), JsonOptions);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            _logger.LogInformation("Providus API Request: {Request}", Redact(jsonContent));

            var response = await client.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation(
                "Providus API Response. StatusCode: {StatusCode}, Reference: {Reference}",
                response.StatusCode, reference);
            _logger.LogInformation("Providus API Response Body: {Response}", responseContent);

            if (!response.IsSuccessStatusCode)
            {
                return new ProvidusApiCall<TResponse>(
                    null,
                    ((int)response.StatusCode).ToString(),
                    $"HTTP Error: {response.StatusCode} - {responseContent}");
            }

            var result = JsonSerializer.Deserialize<TResponse>(responseContent, JsonOptions);

            return result == null
                ? new ProvidusApiCall<TResponse>(null, ProvidusResponseCodes.Error, "Failed to parse response")
                : new ProvidusApiCall<TResponse>(result, string.Empty, string.Empty);
        }
        catch (TaskCanceledException)
        {
            _logger.LogError("Providus API call timed out. Reference: {Reference}", reference);
            return new ProvidusApiCall<TResponse>(null, ProvidusResponseCodes.Timeout, "Request timed out");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Providus API. Reference: {Reference}", reference);
            return new ProvidusApiCall<TResponse>(null, ProvidusResponseCodes.HttpError, $"Connection error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error calling Providus API. Reference: {Reference}", reference);
            return new ProvidusApiCall<TResponse>(null, ProvidusResponseCodes.Error, $"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Strips credentials from a serialized payload before it reaches the logs.
    /// </summary>
    private static string Redact(string json)
    {
        using var document = JsonDocument.Parse(json);

        var redacted = new Dictionary<string, string>();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            redacted[property.Name] = property.Name is "userName" or "password"
                ? "***"
                : property.Value.ToString();
        }

        return JsonSerializer.Serialize(redacted, JsonOptions);
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
                ResponseCode = ProvidusResponseCodes.Timeout,
                ResponseMessage = "MOCK: Request timed out"
            };
        }

        // Default: successful transaction
        return new ProvidusFundTransferResponseDto
        {
            Amount = amount.ToString("F2"),
            TransactionReference = $"PROV{reference}",
            Currency = _settings.CurrencyCode,
            ResponseCode = ProvidusResponseCodes.Success,
            ResponseMessage = "MOCK: OPERATION SUCCESSFUL"
        };
    }

    #endregion
}
