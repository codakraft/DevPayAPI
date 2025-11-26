using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LendingSolution.Application.Repositories.Interfaces;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Application.Helpers;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response.Remita;
using LendingSolution.Core.Models;
using LendingSolution.Core.Settings;
using LendingSolution.Core.Enum;
using LendingSolution.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LendingSolution.Application.Services.Implementations;

public class RemitaService : IRemitaService
{
    private readonly RemitaSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<RemitaService> _logger;
    private readonly ICombinedRepository _cRepo;
    private readonly ApplicationDbContext _db;
    private string? _cachedToken;
    private DateTime? _tokenExpiry;

    public RemitaService(
        IOptions<RemitaSettings> options,
        IHttpClientFactory httpClientFactory,
        ILogger<RemitaService> logger,
        ICombinedRepository cRepo,
        ApplicationDbContext db)
    {
        _settings = options.Value;
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
        _cRepo = cRepo;
        _db = db;
        
        // Log all settings on initialization for debugging
        _logger.LogInformation("RemitaService initialized - BaseUrl={BaseUrl}, UseLiveData={UseLiveData}, ApiKey={ApiKey}, MerchantId={MerchantId}", 
            _settings.BaseUrl, _settings.UseLiveData, 
            string.IsNullOrEmpty(_settings.ApiKey) ? "[EMPTY]" : "[SET]",
            string.IsNullOrEmpty(_settings.MerchantId) ? "[EMPTY]" : "[SET]");
    }
    private void AddStandardHeaders(HttpRequestMessage request, string? token)
    {
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("API_KEY", _settings.ApiKey);
        request.Headers.Add("MERCHANT_ID", _settings.MerchantId);
        request.Headers.Add("REQUEST_ID", Guid.NewGuid().ToString());

        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Add("AUTHORIZATION", token);
    }
    private string BuildUrl(string path) 
    {
        if (string.IsNullOrWhiteSpace(_settings.BaseUrl))
        {
            _logger.LogError("BaseUrl is not configured in RemitaSettings");
            throw new InvalidOperationException("Remita BaseUrl is not configured");
        }
        
        var url = $"{_settings.BaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        _logger.LogDebug("Built URL: {Url}", url);
        return url;
    }
    private string GetPassword() => _settings.Password;
    private string GetUsername() => _settings.Username;
    private async Task<string?> GetAccessTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_cachedToken) && _tokenExpiry > DateTime.UtcNow)
            return _cachedToken;

        var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl(_settings.AuthUrl))
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                username = _settings.Username,
                password = _settings.Password
            }), Encoding.UTF8, "application/json")
        };

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Token request failed: {Status} - {Reason}", response.StatusCode, response.ReasonPhrase);
            return null;
        }

        try
        {
            var auth = JsonSerializer.Deserialize<RemitaAuthResponse>(await response.Content.ReadAsStringAsync());
            if (auth?.AccessToken is null)
            {
                _logger.LogError("Missing token in response");
                return null;
            }

            _cachedToken = auth.AccessToken;
            _tokenExpiry = DateTime.UtcNow.AddMinutes(auth.ExpiresIn ?? 60);
            return _cachedToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token response parse error");
            return null;
        }
    }

    public async Task<object?> GetSalaryHistory(Guid loanId, ReviewHistoryRequestDto body)
    {
        var token = await GetAccessTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Post,
            BuildUrl("/send/api/loansvc/data/api/v2/payday/salary/history/provideCustomerDetails"))
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        AddStandardHeaders(request, token);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Salary history failed: {Status} - {Reason}", response.StatusCode, response.ReasonPhrase);
            return null;
        }

        var result = new ReviewHistoryResponseDto
        {
            CompanyName = "Example Company",
            MaxEligibleAmount = 500000.00m
        };


        return result;

    }

    public async Task<MandateResponse?> GenerateMandate(Guid loanId, SubmitRequestDto body)
    {
        var loan = await _cRepo.GetAllLoanInfoByLoanId(loanId)
                   ?? throw new ArgumentException("Loan not found", nameof(loanId));

        // Get borrower application for account information
        var borrowerApplication = await _db.BorrowerApplications
            .FirstOrDefaultAsync(ba => ba.LoanId == loanId)
            ?? throw new ArgumentException("Borrower application not found for this loan", nameof(loanId));

        var startDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var endDate = DateTime.Now.AddMonths(body.Tenor).AddDays(loan.Product.Moratorium).ToString("yyyy-MM-dd");
        var requestId = Guid.NewGuid().ToString();
        var hash = HashUtils.ComputeSha512Hash($"{_settings.MerchantId}{_settings.ServiceTypeId}{body.Amount}{_settings.ApiKey}");

        var payload = new
        {
            merchantId = _settings.MerchantId,
            serviceTypeId = _settings.ServiceTypeId,
            hash,
            payerName = $"{borrowerApplication.FirstName} {borrowerApplication.LastName}",
            payerEmail = loan.User?.Email ?? string.Empty,
            payerPhone = loan.User?.PhoneNumber ?? string.Empty,
            payerBankCode = borrowerApplication.BankCode,
            payerAccount = borrowerApplication.AccountNo,
            requestId,
            amount = body.Amount,
            startDate,
            endDate,
            mandateType = "SO",
            frequency = "Month"
        };

        var token = await GetAccessTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("/send/api/echannelsvc/echannel/mandate/setup"))
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        AddStandardHeaders(request, token);
        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Mandate request failed: {Status} - {Reason}", response.StatusCode, response.ReasonPhrase);
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<MandateResponse>(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mandate response parse error");
            return null;
        }
    }

    public async Task<InitiateMandateOtpResponseDto?> Initiate(Guid loanId)
    {
        var loan = await _cRepo.GetAllLoanInfoByLoanId(loanId)
                   ?? throw new ArgumentException("Loan not found", nameof(loanId));

        var requestId = Guid.NewGuid().ToString();
        var hash = HashUtils.ComputeSha512Hash($"{_settings.MerchantId}{_settings.ServiceTypeId}{requestId}{_settings.ApiKey}");
        
        var payload = new { mandateId = loan.MandateId, requestId };

        var request = new HttpRequestMessage(HttpMethod.Post,
            BuildUrl("/requestAuthorization"))
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        // Add standard headers
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("API_KEY", _settings.ApiKey);
        request.Headers.Add("MERCHANT_ID", _settings.MerchantId);
        request.Headers.Add("REQUEST_ID", requestId);
        request.Headers.Add("REQUEST_TS", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        request.Headers.Add("API_DETAILS_HASH", hash);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Initiate OTP request failed: {Status} - {Reason}", response.StatusCode, response.ReasonPhrase);
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<InitiateMandateOtpResponseDto>(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Initiate OTP response parse error");
            return null;
        }
    }

    public async Task<ValidateMandateOtpResponseDto?> ValidateMandate(Guid loanId, ValidateMandateOtpRequestDto body)
    {
        var loan = await _cRepo.GetAllLoanInfoByLoanId(loanId)
                   ?? throw new ArgumentException("Loan not found", nameof(loanId));

        var requestId = Guid.NewGuid().ToString();
        var hash = HashUtils.ComputeSha512Hash($"{_settings.MerchantId}{_settings.ServiceTypeId}{requestId}{_settings.ApiKey}");

        var payload = new
        {
            remitaTransRef = loan.RemitaTransRef,
            authParams = new[]
            {
                new { param1 = "OTP", value = body.OtpCode }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post,
            BuildUrl("/validateAuthorization"))
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        // Add specific headers as per API specification
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("API_KEY", _settings.ApiKey);
        request.Headers.Add("MERCHANT_ID", _settings.MerchantId);
        request.Headers.Add("REQUEST_ID", requestId);
        request.Headers.Add("REQUEST_TS", DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString());
        request.Headers.Add("API_DETAILS_HASH", hash);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Validate OTP request failed: {Status} - {Reason}", response.StatusCode, response.ReasonPhrase);
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<ValidateMandateOtpResponseDto>(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Validate OTP response parse error");
            return null;
        }
    }

    public async Task<DebitInstructionResponseDto?> DebitInstruction(Guid loanId, DebitInstructionRequestDto body)
    {
        var loan = await _cRepo.GetAllLoanInfoByLoanId(loanId)
                   ?? throw new ArgumentException("Loan not found", nameof(loanId));

        // Get borrower application for account information
        var borrowerApplication = await _db.BorrowerApplications
            .FirstOrDefaultAsync(ba => ba.LoanId == loanId)
            ?? throw new ArgumentException("Borrower application not found for this loan", nameof(loanId));

        var requestId = Guid.NewGuid().ToString();
        var hash = HashUtils.ComputeSha512Hash($"{_settings.MerchantId}{_settings.ServiceTypeId}{loan.Amount}{_settings.ApiKey}");
        var totalAmount = loan.Amount / loan.DurationInMonths;

        var payload = new
        {
            merchantId = _settings.MerchantId,
            serviceTypeId = _settings.ServiceTypeId,
            hash,
            requestId,
            totalAmount,
            mandateId = loan.MandateId,
            fundingAccount = borrowerApplication.AccountNo,
            fundingBankCode = borrowerApplication.BankCode
        };

        var request = new HttpRequestMessage(HttpMethod.Post,
            BuildUrl("/payment/send"))
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        // Add headers for debit instruction
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Debit instruction failed: {Status} - {Reason}", response.StatusCode, response.ReasonPhrase);
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<DebitInstructionResponseDto>(await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Debit instruction parse error");
            return null;
        }
    }

    public async Task<SalaryHistoryResponse?> GetSalaryHistoryByBvnAsync(SalaryHistoryRequestDto request)
    {
        // Return mock data when UseLiveData is false
        if (!_settings.UseLiveData)
        {
            _logger.LogInformation("UseLiveData=false, returning mock salary history");
            return await Task.FromResult(GetMockSalaryHistory(request));
        }

        var token = await GetAccessTokenAsync();
        if (token == null)
        {
            _logger.LogError("Failed to get access token for salary history request");
            return null;
        }

        var payload = new
        {
            authorisationCode = request.AuthorisationCode ?? "",
            firstName = request.FirstName,
            lastName = request.LastName,
            middleName = request.MiddleName ?? "R ",
            accountNumber = request.AccountNumber,
            bankCode = request.BankCode,
            bvn = request.Bvn,
            authorisationChannel = request.AuthorisationChannel ?? "USSD"
        };

        var requestUrl = BuildUrl("/send/api/loansvc/data/api/v2/payday/salary/history/provideCustomerDetails");
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        // Use specific headers for salary history endpoint as per API documentation
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        httpRequest.Headers.Add("API_KEY", _settings.ApiKey);
        httpRequest.Headers.Add("MERCHANT_ID", _settings.MerchantId);
        httpRequest.Headers.Add("REQUEST_ID", Guid.NewGuid().ToString());
        
        if (!string.IsNullOrWhiteSpace(token))
            httpRequest.Headers.Add("AUTHORIZATION", token);

        _logger.LogInformation("Sending salary history request to: {Url}", requestUrl);

        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Salary history request failed: {Status} - {Reason}", 
                response.StatusCode, response.ReasonPhrase);
            return null;
        }

        try
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<SalaryHistoryResponse>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing salary history response");
            return null;
        }
    }

    public async Task<AccountVerificationResponseDto?> VerifyAccountAsync(AccountVerificationRequestDto request)
    {
        // Return mock data when UseLiveData is false
        if (!_settings.UseLiveData)
        {
            _logger.LogInformation("UseLiveData=false, returning mock account verification");
            return await Task.FromResult(GetMockAccountVerification(request));
        }

        var token = await GetAccessTokenAsync();
        if (token == null)
        {
            _logger.LogError("Failed to get access token for account verification");
            return null;
        }

        var payload = new
        {
            accountNumber = request.AccountNumber,
            bankCode = request.BankCode,
            accountName = request.AccountName,
            bvn = request.Bvn
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post,
            BuildUrl("/send/api/loansvc/data/api/v2/payday/account/verify"))
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        AddStandardHeaders(httpRequest, token);

        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Account verification failed: {Status} - {Reason}", 
                response.StatusCode, response.ReasonPhrase);
            return null;
        }

        try
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<AccountVerificationResponseDto>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing account verification response");
            return null;
        }
    }

    public async Task<CreateMandateResponseDto?> CreateLoanMandateAsync(Guid loanId, CreateMandateRequestDto request, string? userId)
    {
        var loan = await _cRepo.GetAllLoanInfoByLoanId(loanId);
        if (loan == null)
        {
            _logger.LogError("Loan not found: {LoanId}", loanId);
            return null;
        }

        var token = await GetAccessTokenAsync();
        if (token == null)
        {
            _logger.LogError("Failed to get access token for mandate creation");
            return null;
        }

        var requestId = Guid.NewGuid().ToString();
        var hash = HashUtils.ComputeSha512Hash($"{_settings.MerchantId}{_settings.ServiceTypeId}{request.Amount}{_settings.ApiKey}");

        var payload = new
        {
            merchantId = _settings.MerchantId,
            serviceTypeId = _settings.ServiceTypeId,
            hash,
            payerName = request.PayerName,
            payerEmail = request.PayerEmail,
            payerPhone = request.PayerPhone,
            payerBankCode = request.PayerBankCode,
            payerAccount = request.PayerAccount,
            requestId,
            amount = request.Amount,
            startDate = request.StartDate.ToString("yyyy-MM-dd"),
            endDate = request.EndDate.ToString("yyyy-MM-dd"),
            mandateType = request.MandateType,
            frequency = request.Frequency
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post,
            BuildUrl("/send/api/echannelsvc/echannel/mandate/setup"))
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        AddStandardHeaders(httpRequest, token);

        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Mandate creation failed: {Status} - {Reason}", 
                response.StatusCode, response.ReasonPhrase);
            return null;
        }

        try
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<CreateMandateResponseDto>(responseContent);
            
            // Update loan with mandate ID if successful
            if (result != null && !string.IsNullOrEmpty(result.MandateId))
            {
                loan.MandateId = result.MandateId;
                loan.RemitaTransRef = result.RemitaTransRef;
                await _cRepo.UpdateLoanAsync(loan);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing mandate creation response");
            return null;
        }
    }

    public async Task<DisbursementResponseDto?> ProcessLoanDisbursementAsync(Guid loanId, RemitaDisbursementRequestDto request, string? userId)
    {
        // Return mock data when UseLiveData is false
        if (!_settings.UseLiveData)
        {
            _logger.LogInformation("UseLiveData=false, returning mock disbursement");
            return await Task.FromResult(GetMockDisbursement(request));
        }

        var loan = await _cRepo.GetAllLoanInfoByLoanId(loanId);
        if (loan == null)
        {
            _logger.LogError("Loan not found: {LoanId}", loanId);
            return null;
        }

        var token = await GetAccessTokenAsync();
        if (token == null)
        {
            _logger.LogError("Failed to get access token for disbursement");
            return null;
        }

        var requestId = Guid.NewGuid().ToString();
        var hash = HashUtils.ComputeSha512Hash($"{_settings.MerchantId}{_settings.ServiceTypeId}{request.Amount}{_settings.ApiKey}");

        var payload = new
        {
            merchantId = _settings.MerchantId,
            serviceTypeId = _settings.ServiceTypeId,
            hash,
            requestId,
            amount = request.Amount,
            beneficiaryAccount = request.BeneficiaryAccount,
            beneficiaryBankCode = request.BeneficiaryBankCode,
            beneficiaryName = request.BeneficiaryName,
            narration = request.Narration,
            reference = request.Reference ?? requestId,
            debitAccount = request.DebitAccount,
            debitBankCode = request.DebitBankCode,
            valueDate = request.ValueDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd")
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post,
            BuildUrl("/send/api/loansvc/data/api/v2/payday/disbursement/process"))
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        AddStandardHeaders(httpRequest, token);

        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Disbursement failed: {Status} - {Reason}", 
                response.StatusCode, response.ReasonPhrase);
            return null;
        }

        try
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<DisbursementResponseDto>(responseContent);
            
            // Update loan status if successful
            if (result != null && result.TransactionStatus == "SUCCESS")
            {
                loan.Status = LoanStatus.Disbursed;
                loan.DisbursementDate = DateTime.UtcNow;
                loan.DisbursementReference = result.TransactionRef;
                await _cRepo.UpdateLoanAsync(loan);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing disbursement response");
            return null;
        }
    }

    public async Task<RepaymentCollectionResponseDto?> CollectRepaymentAsync(Guid loanId, RepaymentCollectionRequestDto request, string? userId)
    {
        // Return mock data when UseLiveData is false
        if (!_settings.UseLiveData)
        {
            _logger.LogInformation("UseLiveData=false, returning mock repayment collection");
            return await Task.FromResult(GetMockRepaymentCollection(request));
        }

        var loan = await _cRepo.GetAllLoanInfoByLoanId(loanId);
        if (loan == null)
        {
            _logger.LogError("Loan not found: {LoanId}", loanId);
            return null;
        }

        var token = await GetAccessTokenAsync();
        if (token == null)
        {
            _logger.LogError("Failed to get access token for repayment collection");
            return null;
        }

        var requestId = Guid.NewGuid().ToString();
        var hash = HashUtils.ComputeSha512Hash($"{_settings.MerchantId}{_settings.ServiceTypeId}{request.Amount}{_settings.ApiKey}");

        var payload = new
        {
            merchantId = _settings.MerchantId,
            serviceTypeId = _settings.ServiceTypeId,
            hash,
            requestId,
            amount = request.Amount,
            mandateId = request.MandateId,
            payerAccount = request.PayerAccount,
            payerBankCode = request.PayerBankCode,
            description = request.Description,
            reference = request.Reference ?? requestId,
            collectionDate = request.CollectionDate?.ToString("yyyy-MM-dd") ?? DateTime.Now.ToString("yyyy-MM-dd"),
            collectionType = request.CollectionType
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post,
            BuildUrl("/send/api/echannelsvc/echannel/mandate/payment/send"))
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        AddStandardHeaders(httpRequest, token);

        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Repayment collection failed: {Status} - {Reason}", 
                response.StatusCode, response.ReasonPhrase);
            return null;
        }

        try
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RepaymentCollectionResponseDto>(responseContent);
            
            // Log repayment attempt
            if (result != null)
            {
                _logger.LogInformation("Repayment collection processed for loan {LoanId}: {Status}", 
                    loanId, result.CollectionStatus);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing repayment collection response");
            return null;
        }
    }

    public async Task<TransactionStatusResponseDto?> GetTransactionStatusAsync(string transactionRef)
    {
        // Return mock data when UseLiveData is false
        if (!_settings.UseLiveData)
        {
            _logger.LogInformation("UseLiveData=false, returning mock transaction status");
            return await Task.FromResult(GetMockTransactionStatus(transactionRef));
        }

        var token = await GetAccessTokenAsync();
        if (token == null)
        {
            _logger.LogError("Failed to get access token for transaction status check");
            return null;
        }

        var httpRequest = new HttpRequestMessage(HttpMethod.Get,
            BuildUrl($"/send/api/loansvc/data/api/v2/payday/transaction/status/{transactionRef}"));

        AddStandardHeaders(httpRequest, token);

        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Transaction status check failed: {Status} - {Reason}", 
                response.StatusCode, response.ReasonPhrase);
            return null;
        }

        try
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TransactionStatusResponseDto>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing transaction status response");
            return null;
        }
    }

    public async Task<BanksResponseDto?> GetBanksAsync()
    {
        // Return mock data when UseLiveData is false
        if (!_settings.UseLiveData)
        {
            _logger.LogInformation("UseLiveData=false, returning mock banks data");
            return await Task.FromResult(GetMockBanks());
        }

        var token = await GetAccessTokenAsync();
        if (token == null)
        {
            _logger.LogError("Failed to get access token for banks list");
            return null;
        }

        var httpRequest = new HttpRequestMessage(HttpMethod.Get,
            BuildUrl("/send/api/loansvc/data/api/v2/payday/banks"));

        AddStandardHeaders(httpRequest, token);

        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Banks list request failed: {Status} - {Reason}", 
                response.StatusCode, response.ReasonPhrase);
            return null;
        }

        try
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<BanksResponseDto>(responseContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing banks response");
            return null;
        }
    }

    public async Task<StopMandateResponseDto?> StopMandate(Guid loanId)
    {
        var loan = await _cRepo.GetAllLoanInfoByLoanId(loanId);
        if (loan == null)
        {
            _logger.LogError("Loan not found: {LoanId}", loanId);
            return null;
        }

        if (string.IsNullOrEmpty(loan.MandateId))
        {
            _logger.LogError("Mandate ID not found for loan: {LoanId}", loanId);
            return null;
        }

        var requestId = Guid.NewGuid().ToString();
        var hash = HashUtils.ComputeSha512Hash($"{_settings.MerchantId}{loan.MandateId}{requestId}{_settings.ApiKey}");

        var payload = new
        {
            merchantId = _settings.MerchantId,
            hash,
            mandateId = loan.MandateId,
            requestId
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post,
            BuildUrl("/send/api/echannelsvc/echannel/mandate/stop"))
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

        // Add headers for stop mandate
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(httpRequest);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Stop mandate request failed: {Status} - {Reason}", 
                response.StatusCode, response.ReasonPhrase);
            return null;
        }

        try
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<StopMandateResponseDto>(responseContent);
            
            // Update loan mandate status if successful
            if (result != null && result.Status == "SUCCESS")
            {
                loan.MandateStatus = "STOPPED";
                loan.MandateStoppedDate = DateTime.UtcNow;
                await _cRepo.UpdateLoanAsync(loan);
                
                _logger.LogInformation("Mandate stopped for loan {LoanId}: {MandateId}", 
                    loanId, loan.MandateId);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing stop mandate response");
            return null;
        }
    }

    public async Task<bool> ProcessWebhookNotificationAsync(RemitaWebhookNotificationDto notification)
    {
        try
        {
            _logger.LogInformation("Processing Remita webhook notification: {Type} - {TransactionRef}", 
                notification.NotificationType, notification.TransactionRef);

            // Verify webhook authenticity using hash
            if (!string.IsNullOrEmpty(notification.Hash))
            {
                var expectedHash = HashUtils.ComputeSha512Hash(
                    $"{notification.MerchantId}{notification.ServiceTypeId}{notification.Amount}{_settings.ApiKey}");
                
                if (notification.Hash != expectedHash)
                {
                    _logger.LogWarning("Webhook hash verification failed for transaction: {TransactionRef}", 
                        notification.TransactionRef);
                    return false;
                }
            }

            // Process based on notification type
            switch (notification.NotificationType.ToUpper())
            {
                case "MANDATE_ACTIVATION":
                    await ProcessMandateActivationNotification(notification);
                    break;
                case "DISBURSEMENT_CONFIRMATION":
                    await ProcessDisbursementConfirmationNotification(notification);
                    break;
                case "REPAYMENT_COLLECTION":
                    await ProcessRepaymentCollectionNotification(notification);
                    break;
                case "TRANSACTION_STATUS_UPDATE":
                    await ProcessTransactionStatusUpdateNotification(notification);
                    break;
                default:
                    _logger.LogWarning("Unknown webhook notification type: {Type}", notification.NotificationType);
                    break;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook notification: {TransactionRef}", 
                notification.TransactionRef);
            return false;
        }
    }

    private async Task ProcessMandateActivationNotification(RemitaWebhookNotificationDto notification)
    {
        if (string.IsNullOrEmpty(notification.MandateId))
        {
            _logger.LogWarning("Mandate activation notification missing mandate ID");
            return;
        }

        var loan = await _cRepo.GetLoanByMandateIdAsync(notification.MandateId);
        if (loan != null)
        {
            loan.MandateStatus = notification.Status;
            loan.MandateActivationDate = notification.TransactionDate;
            await _cRepo.UpdateLoanAsync(loan);
            
            _logger.LogInformation("Updated mandate status for loan {LoanId}: {Status}", 
                loan.Id, notification.Status);
        }
    }

    private async Task ProcessDisbursementConfirmationNotification(RemitaWebhookNotificationDto notification)
    {
        var loan = await _cRepo.GetLoanByTransactionRefAsync(notification.TransactionRef);
        if (loan != null)
        {
            if (notification.Status.ToUpper() == "SUCCESS")
            {
                loan.Status = LoanStatus.Disbursed;
                loan.DisbursementDate = notification.TransactionDate;
                loan.DisbursementReference = notification.RemitaTransRef;
            }
            else
            {
                loan.Status = LoanStatus.Rejected;
                loan.FailureReason = notification.ResponseMessage;
            }

            await _cRepo.UpdateLoanAsync(loan);
            
            _logger.LogInformation("Updated disbursement status for loan {LoanId}: {Status}", 
                loan.Id, notification.Status);
        }
    }

    private async Task ProcessRepaymentCollectionNotification(RemitaWebhookNotificationDto notification)
    {
        if (string.IsNullOrEmpty(notification.MandateId))
        {
            _logger.LogWarning("Repayment collection notification missing mandate ID");
            return;
        }

        var loan = await _cRepo.GetLoanByMandateIdAsync(notification.MandateId);
        if (loan != null)
        {
            // Create repayment record
            var repayment = new Repayment
            {
                Id = Guid.NewGuid(),
                LoanId = loan.Id.ToString(),
                Amount = notification.Amount,
                RepaymentDate = notification.TransactionDate,
                Status = notification.Status,
                TransactionReference = notification.RemitaTransRef,
                CreatedAt = DateTime.UtcNow
            };

            await _cRepo.CreateRepaymentAsync(repayment);
            
            _logger.LogInformation("Created repayment record for loan {LoanId}: {Amount}", 
                loan.Id, notification.Amount);
        }
    }

    private Task ProcessTransactionStatusUpdateNotification(RemitaWebhookNotificationDto notification)
    {
        // Log the status update for monitoring purposes
        _logger.LogInformation("Transaction status update: {TransactionRef} - {Status}", 
            notification.TransactionRef, notification.Status);
            
        // Additional processing can be added here based on business requirements
        return Task.CompletedTask;
    }

    public async Task<RemitaSalaryHistoryResponseDto?> GetBorrowerSalaryHistoryAsync(string accountNumber, string bankCode, string bvn)
    {
        // Return mock data when UseLiveData is false
        if (!_settings.UseLiveData)
        {
            _logger.LogInformation("UseLiveData=false, returning mock borrower salary history");
            return await Task.FromResult(GetMockBorrowerSalaryHistory(accountNumber, bankCode, bvn));
        }

        try
        {
            var accessToken = await GetAccessTokenAsync();
            if (accessToken == null)
            {
                _logger.LogError("Failed to get access token for salary history");
                return null;
            }

            var requestId = Guid.NewGuid().ToString();
            var hash = HashUtils.ComputeSha512Hash($"{_settings.MerchantId}{accountNumber}{bankCode}{_settings.ApiKey}");

            var payload = new
            {
                merchantId = _settings.MerchantId,
                accountNumber,
                bankCode,
                bvn,
                hash,
                requestId
            };

            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.DefaultRequestHeaders.Add("Content-Type", "application/json");

            var jsonPayload = JsonSerializer.Serialize(payload);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var apiUrl = $"{_settings.BaseUrl}/v1/salary/history/direct";
            var response = await httpClient.PostAsync(apiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<RemitaSalaryHistoryResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _logger.LogInformation("Successfully retrieved salary history for account: {AccountNumber}", accountNumber);
                return result;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to get salary history. Status: {StatusCode}, Response: {Response}", 
                    response.StatusCode, errorContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting salary history for account: {AccountNumber}", accountNumber);
            return null;
        }
    }

    #region Mock Data Methods

    private BanksResponseDto GetMockBanks()
    {
        _logger.LogInformation("Returning mock banks data");
        
        return new BanksResponseDto
        {
            Status = "success",
            Message = "Banks retrieved successfully (Mock Data)",
            Banks = new List<BankDto>
            {
                new BankDto { BankCode = "058", BankName = "GUARANTY TRUST BANK", Type = "Commercial", IsActive = true },
                new BankDto { BankCode = "057", BankName = "ZENITH BANK PLC", Type = "Commercial", IsActive = true },
                new BankDto { BankCode = "044", BankName = "ACCESS BANK PLC", Type = "Commercial", IsActive = true },
                new BankDto { BankCode = "033", BankName = "UNITED BANK FOR AFRICA PLC", Type = "Commercial", IsActive = true },
                new BankDto { BankCode = "011", BankName = "FIRST BANK OF NIGERIA PLC", Type = "Commercial", IsActive = true },
                new BankDto { BankCode = "214", BankName = "FIRST CITY MONUMENT BANK PLC", Type = "Commercial", IsActive = true },
                new BankDto { BankCode = "232", BankName = "STERLING BANK PLC", Type = "Commercial", IsActive = true },
                new BankDto { BankCode = "035", BankName = "WEMA BANK PLC", Type = "Commercial", IsActive = true },
                new BankDto { BankCode = "070", BankName = "FIDELITY BANK PLC", Type = "Commercial", IsActive = true },
                new BankDto { BankCode = "050", BankName = "ECOBANK NIGERIA PLC", Type = "Commercial", IsActive = true }
            },
            TotalCount = 10,
            LastUpdated = DateTime.UtcNow
        };
    }

    private SalaryHistoryResponse GetMockSalaryHistory(SalaryHistoryRequestDto request)
    {
        _logger.LogInformation("Returning mock salary history for BVN: {BVN}", request.Bvn);
        
        return new SalaryHistoryResponse
        {
            Status = "success",
            Message = "Salary history retrieved successfully (Mock Data)",
            Data = new SalaryHistoryReviewData
            {
                CompanyName = "Test Company Ltd",
                CustomerName = "John Doe"
            }
        };
    }

    private AccountVerificationResponseDto GetMockAccountVerification(AccountVerificationRequestDto request)
    {
        _logger.LogInformation("Returning mock account verification for Account: {Account}", request.AccountNumber);
        
        return new AccountVerificationResponseDto
        {
            Status = "success",
            Message = "Account verified successfully (Mock Data)",
            AccountNumber = request.AccountNumber,
            AccountName = "John Doe",
            BankCode = request.BankCode,
            BankName = "GUARANTY TRUST BANK",
            IsActive = true,
            AccountType = "Savings",
            Bvn = "22222222222",
            DateOfBirth = DateTime.UtcNow.AddYears(-30),
            PhoneNumber = "08012345678",
            Email = "john.doe@example.com",
            Address = "123 Test Street, Lagos",
            Gender = "Male"
        };
    }

    private CreateMandateResponseDto GetMockMandateCreation(CreateMandateRequestDto request)
    {
        _logger.LogInformation("Returning mock mandate creation for Payer: {Payer}", request.PayerName);
        
        return new CreateMandateResponseDto
        {
            Status = "success",
            Message = "Mandate created successfully (Mock Data)",
            MandateId = $"MOCK_MANDATE_{Guid.NewGuid().ToString()[..8].ToUpper()}",
            RemitaTransRef = $"REM{DateTime.UtcNow.Ticks}",
            RequestId = $"REQ{DateTime.UtcNow.Ticks}",
            Amount = request.Amount,
            PayerAccount = request.PayerAccount,
            PayerBankCode = request.PayerBankCode,
            PayerName = request.PayerName,
            MandateType = request.MandateType,
            Frequency = request.Frequency,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            MandateStatus = "Active",
            Description = request.Description ?? "Loan Repayment Mandate",
            ResponseCode = "00",
            ResponseMessage = "Successful",
            CreatedDate = DateTime.UtcNow,
            RequiresOtp = false
        };
    }

    private DisbursementResponseDto GetMockDisbursement(RemitaDisbursementRequestDto request)
    {
        _logger.LogInformation("Returning mock disbursement for Account: {Account}", request.BeneficiaryAccount);
        
        return new DisbursementResponseDto
        {
            Status = "success",
            Message = "Disbursement processed successfully (Mock Data)",
            TransactionRef = $"TXN{DateTime.UtcNow.Ticks}",
            RemitaTransRef = $"MOCK_DISB_{Guid.NewGuid().ToString()[..8].ToUpper()}",
            Amount = request.Amount,
            BeneficiaryAccount = request.BeneficiaryAccount,
            BeneficiaryName = request.BeneficiaryName,
            BeneficiaryBankCode = request.BeneficiaryBankCode,
            BeneficiaryBankName = "GUARANTY TRUST BANK",
            DebitAccount = request.DebitAccount,
            DebitBankCode = request.DebitBankCode,
            TransactionStatus = "Successful",
            TransactionDate = DateTime.UtcNow,
            Narration = request.Narration,
            Reference = request.Reference,
            Fees = 50.00m,
            ResponseCode = "00",
            ResponseMessage = "Successful"
        };
    }

    private RepaymentCollectionResponseDto GetMockRepaymentCollection(RepaymentCollectionRequestDto request)
    {
        _logger.LogInformation("Returning mock repayment collection");
        
        return new RepaymentCollectionResponseDto
        {
            Status = "success",
            Message = "Repayment collected successfully (Mock Data)",
            TransactionRef = $"REP{DateTime.UtcNow.Ticks}",
            RemitaTransRef = $"MOCK_REP_{Guid.NewGuid().ToString()[..8].ToUpper()}",
            MandateId = request.MandateId,
            Amount = request.Amount,
            PayerAccount = "0123456789",
            PayerBankCode = "058",
            PayerName = "John Doe",
            CollectionStatus = "Successful",
            CollectionDate = DateTime.UtcNow,
            Description = "Loan repayment collection",
            Reference = $"REF{DateTime.UtcNow.Ticks}",
            CollectionType = "Mandate",
            Fees = 25.00m,
            ResponseCode = "00",
            ResponseMessage = "Successful"
        };
    }

    private TransactionStatusResponseDto GetMockTransactionStatus(string transactionRef)
    {
        _logger.LogInformation("Returning mock transaction status for: {Ref}", transactionRef);
        
        return new TransactionStatusResponseDto
        {
            Status = "success",
            Message = "Transaction status retrieved successfully (Mock Data)",
            TransactionRef = transactionRef,
            RemitaTransRef = $"REM{DateTime.UtcNow.Ticks}",
            TransactionStatus = "Successful",
            Amount = 100000,
            TransactionDate = DateTime.UtcNow.AddHours(-2),
            MandateId = "MOCK_MANDATE_12345",
            PayerAccount = "0123456789",
            PayerBankCode = "058",
            PayerName = "John Doe",
            BeneficiaryAccount = "9876543210",
            BeneficiaryBankCode = "057",
            BeneficiaryName = "Test Company",
            TransactionType = "Mandate Collection",
            Description = "Mock transaction completed successfully",
            Reference = $"REF{DateTime.UtcNow.Ticks}",
            Fees = 25.00m,
            ResponseCode = "00",
            ResponseMessage = "Successful",
            CompletedDate = DateTime.UtcNow.AddHours(-1),
            FailureReason = null
        };
    }

    private RemitaSalaryHistoryResponseDto GetMockBorrowerSalaryHistory(string accountNumber, string bankCode, string bvn)
    {
        _logger.LogInformation("Returning mock borrower salary history for Account: {Account}", accountNumber);
        
        return new RemitaSalaryHistoryResponseDto
        {
            Status = "success",
            HasData = true,
            ResponseId = $"RESP{DateTime.UtcNow.Ticks}",
            ResponseDate = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
            RequestDate = DateTime.UtcNow.AddSeconds(-5).ToString("yyyy-MM-dd HH:mm:ss"),
            ResponseCode = "00",
            ResponseMsg = "Successful",
            Data = new RemitaSalaryDataDto
            {
                CustomerId = $"CUST{DateTime.UtcNow.Ticks}",
                AccountNumber = accountNumber,
                BankCode = bankCode,
                BVN = bvn,
                CompanyName = "Test Company Ltd",
                CustomerName = "John Doe",
                Category = "Salary Earner",
                FirstPaymentDate = DateTime.UtcNow.AddMonths(-6).ToString("yyyy-MM-dd"),
                SalaryCount = "6",
                SalaryPaymentDetails = new List<RemitaSalaryPaymentDto>
                {
                    new RemitaSalaryPaymentDto 
                    { 
                        PaymentDate = DateTime.UtcNow.AddDays(-15).ToString("yyyy-MM-dd"), 
                        Amount = "350000", 
                        AccountNumber = accountNumber, 
                        BankCode = bankCode 
                    },
                    new RemitaSalaryPaymentDto 
                    { 
                        PaymentDate = DateTime.UtcNow.AddMonths(-1).AddDays(-15).ToString("yyyy-MM-dd"), 
                        Amount = "350000", 
                        AccountNumber = accountNumber, 
                        BankCode = bankCode 
                    },
                    new RemitaSalaryPaymentDto 
                    { 
                        PaymentDate = DateTime.UtcNow.AddMonths(-2).AddDays(-15).ToString("yyyy-MM-dd"), 
                        Amount = "350000", 
                        AccountNumber = accountNumber, 
                        BankCode = bankCode 
                    },
                    new RemitaSalaryPaymentDto 
                    { 
                        PaymentDate = DateTime.UtcNow.AddMonths(-3).AddDays(-15).ToString("yyyy-MM-dd"), 
                        Amount = "350000", 
                        AccountNumber = accountNumber, 
                        BankCode = bankCode 
                    },
                    new RemitaSalaryPaymentDto 
                    { 
                        PaymentDate = DateTime.UtcNow.AddMonths(-4).AddDays(-15).ToString("yyyy-MM-dd"), 
                        Amount = "350000", 
                        AccountNumber = accountNumber, 
                        BankCode = bankCode 
                    },
                    new RemitaSalaryPaymentDto 
                    { 
                        PaymentDate = DateTime.UtcNow.AddMonths(-5).AddDays(-15).ToString("yyyy-MM-dd"), 
                        Amount = "350000", 
                        AccountNumber = accountNumber, 
                        BankCode = bankCode 
                    }
                },
                LoanHistoryDetails = new List<RemitaLoanHistoryDto>
                {
                    new RemitaLoanHistoryDto 
                    { 
                        LoanProvider = "Mock Bank", 
                        LoanAmount = 500000, 
                        OutstandingAmount = 0, 
                        Status = "Paid", 
                        LoanDisbursementDate = DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd"),
                        RepaymentAmount = 50000,
                        RepaymentFreq = "Monthly"
                    }
                }
            }
        };
    }

    #endregion
}




