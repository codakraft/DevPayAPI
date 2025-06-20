using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos.Response.Remita;
using LendingSolution.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LendingSolution.Application.Services.Implementations;

public class RemitaService : IRemitaService
{
    private readonly RemitaSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<RemitaService> _logger;

    private string? _cachedToken;
    private DateTime? _tokenExpiry;

    public RemitaService(
        IOptions<RemitaSettings> options,
        IHttpClientFactory httpClientFactory,
        ILogger<RemitaService> logger)
    {
        _settings = options.Value;
        _httpClient = httpClientFactory.CreateClient();
        _logger = logger;
    }

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
    public string GetPassword() => _settings.Password;

    public async Task<SalaryHistoryResponse?> GetSalaryHistoryAsync(object requestBody)
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
}
