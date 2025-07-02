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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LendingSolution.Application.Services.Implementations;

public class RemitaService(
    IOptions<RemitaSettings> options,
    IHttpClientFactory httpClientFactory,
    ILogger<RemitaService> logger,
    ICombinedRepository cRepo
) : IRemitaService
{
    private readonly RemitaSettings _settings = options.Value;
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    private readonly ILogger<RemitaService> _logger = logger;
    private readonly ICombinedRepository _cRepo = cRepo;
    private string? _cachedToken;
    private DateTime? _tokenExpiry;
    private void AddStandardHeaders(HttpRequestMessage request, string? token)
    {
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Add("API_KEY", _settings.ApiKey);
        request.Headers.Add("MERCHANT_ID", _settings.MerchantId);
        request.Headers.Add("REQUEST_ID", Guid.NewGuid().ToString());

        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Add("AUTHORIZATION", token);
    }
    private string BuildUrl(string path) => $"{_settings.BaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
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

        var startDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var endDate = DateTime.Now.AddMonths(body.Tenor).AddDays(loan.Product.Moratorium).ToString("yyyy-MM-dd");
        var requestId = Guid.NewGuid().ToString();
        var hash = HashUtils.ComputeSha512Hash($"{_settings.MerchantId}{_settings.ServiceTypeId}{body.Amount}{_settings.ApiKey}");

        var payload = new
        {
            merchantId = _settings.MerchantId,
            serviceTypeId = _settings.ServiceTypeId,
            hash,
            payerName = loan.Account.AccountName,
            payerEmail = loan.User.Email,
            payerPhone = loan.User.PhoneNumber,
            payerBankCode = loan.Account.BankCode,
            payeraccount = loan.Account.AccountNumber,
            requestId,
            amount = body.Amount,
            mandateType = "SO",
            Frequency = "Month",
            StartDate = startDate,
            endDate
        };

        var token = await GetAccessTokenAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, BuildUrl("/send/api/loansvc/data/api/v2/payday/mandate/create"))
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
        var payload = new { mandateId = loan.MandateId, requestId };

        var request = new HttpRequestMessage(HttpMethod.Post,
            BuildUrl("/send/api/echannelsvc/echannel/mandate/requestAuthorization"))
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

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

        var payload = new
        {
            remitaTransRef = loan.RemitaTransRef,
            authParams = new[]
            {
                new { param1 = "OTP", value = body.OtpCode }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post,
            BuildUrl("/send/api/echannelsvc/echannel/mandate/validateAuthorization"))
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

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
            fundingAccount = loan.Account.AccountNumber,
            fundingBankCode = loan.Account.BankCode
        };

        var request = new HttpRequestMessage(HttpMethod.Post,
            BuildUrl("/send/api/echannelsvc/echannel/mandate/payment/send"))
        {
            Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        };

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
}


