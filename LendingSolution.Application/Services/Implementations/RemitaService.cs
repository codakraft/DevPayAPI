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
    private readonly ICombinedRepository _cRepo = cRepo;
    private readonly ILogger<RemitaService> _logger = logger;
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
    public string GetUsername() => _settings.Username;
    private string GetPassword() => _settings.Password;
    private string BuildUrl(string relativePath) =>
        $"{_settings.BaseUrl.TrimEnd('/')}/{relativePath.TrimStart('/')}";
    private async Task<string?> GetAccessTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_cachedToken) && _tokenExpiry > DateTime.UtcNow)
            return _cachedToken;

        var url = BuildUrl(_settings.AuthUrl);
        var credentials = new
        {
            username = _settings.Username,
            password = _settings.Password
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(credentials), Encoding.UTF8, "application/json")
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Remita token request failed: {Status} - {Reason}", response.StatusCode, response.ReasonPhrase);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();

        try
        {
            var auth = JsonSerializer.Deserialize<RemitaAuthResponse>(json);
            if (auth?.AccessToken is null)
            {
                _logger.LogError("Access token was missing from Remita response.");
                return null;
            }

            _cachedToken = auth.AccessToken;
            _tokenExpiry = DateTime.UtcNow.AddMinutes(auth.ExpiresIn ?? 60);
            return _cachedToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse token response from Remita");
            return null;
        }
    }
    public async Task<SalaryHistoryResponse?> GetSalaryHistory(object requestBody)
    {
        var token = await GetAccessTokenAsync();
        var url = BuildUrl("/send/api/loansvc/data/api/v2/payday/salary/history/provideCustomerDetails");

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        };

        AddStandardHeaders(request, token);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Remita salary history request failed: {Status} - {Reason}", response.StatusCode, response.ReasonPhrase);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();

        try
        {
            return JsonSerializer.Deserialize<SalaryHistoryResponse>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse salary history response");
            return null;
        }
    }
    public async Task<MandateResponse?> GenerateMandate(Guid loanId, SubmitRequestDto requestBody)
    {
        var loan = await _cRepo.GetAllLoanInfoByLoanId(loanId);

        if (loan is null)
            throw new ArgumentException("Loan not found", nameof(loanId));

        String startDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
        String endDate = DateTime.Now.AddMonths(requestBody.Tenor).AddDays(loan.Product.Moratorium).ToString("yyyy-MM-dd");
        String requiestId = new Guid().ToString();
        String hash = HashUtils.ComputeSha512Hash($"{_settings.MerchantId}{_settings.ServiceTypeId}{requestBody.Amount}{_settings.ApiKey}");

        var newRequestBody = new
        {
            MerchantId = _settings.MerchantId,
            ServiceTypeId = _settings.ServiceTypeId,
            hash = _settings.Hash,
            payerName = loan.Account.AccountName,
            payerEmail = loan.User.Email,
            payerPhone = loan.User.PhoneNumber,
            payerBankCode = loan.Account.BankCode,
            payeraccount = loan.Account.AccountNumber,
            requestId = requiestId.ToString(),
            amount = requestBody.Amount,
            mandateType = "SO",
            Frequency = "Month",
            StartDate = startDate,
            endDate = endDate,
        };

        var token = await GetAccessTokenAsync();
        var url = BuildUrl("/send/api/loansvc/data/api/v2/payday/mandate/create");

        var request = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
        };

        AddStandardHeaders(request, token);

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Remita mandate creation request failed: {Status} - {Reason}", response.StatusCode, response.ReasonPhrase);
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();

        try
        {
            return JsonSerializer.Deserialize<MandateResponse>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse mandate creation response");
            return null;
        }
    }
}
