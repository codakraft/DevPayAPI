using System.Net.Http.Headers;
using System.Security.Cryptography;
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

        _logger.LogInformation("RemitaService initialized - BaseUrl={BaseUrl}, UseLiveData={UseLiveData}",
            _settings.BaseUrl, _settings.UseLiveData);
    }

    // Salary History Method
    public async Task<RemitaSalaryHistoryResponseDto?> GetSalaryHistoryAsync(
        string accountNumber,
        string bankCode,
        string bvn,
        string email,
        string firstName = "",
        string lastName = "",
        string middleName = "",
        string? authorisationCode = null
    )
    {
        try
        {
            // Handle authorization code based on mode and email
            string finalAuthCode = authorisationCode ?? string.Empty;

            if (string.IsNullOrWhiteSpace(finalAuthCode) && !string.IsNullOrWhiteSpace(email))
            {
                // Check if customer exists in RemitaCustomer table
                var existingCustomer = await _db.RemitaCustomers
                    .FirstOrDefaultAsync(rc => rc.Email.ToLower() == email.ToLower());

                if (existingCustomer != null)
                {
                    _logger.LogInformation("Found existing Remita customer for email {Email}, using stored authorization code", email);
                    finalAuthCode = existingCustomer.AuthorisationCode;
                    
                    // Update last used timestamp
                    existingCustomer.LastUsedAt = DateTime.UtcNow;
                    existingCustomer.UpdatedAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
                else
                {
                    // Generate new authorization code and customer ID
                    finalAuthCode = GenerateRandomAuthorizationCode();
                    var customerId = GenerateCustomerId();

                    _logger.LogInformation("Creating new Remita customer for email {Email} with CustomerId {CustomerId}", email, customerId);

                    // Create new RemitaCustomer record
                    var newCustomer = new RemitaCustomer
                    {
                        Email = email,
                        CustomerId = customerId,
                        AuthorisationCode = finalAuthCode,
                        LastUsedAt = DateTime.UtcNow
                    };

                    _db.RemitaCustomers.Add(newCustomer);
                    await _db.SaveChangesAsync();
                }
            }

            // Build payload
            var payload = new
            {
                authorisationCode = finalAuthCode,
                firstName,
                lastName,
                middleName,
                accountNumber,
                bankCode,
                bvn,
                authorisationChannel = "USSD"
            };

            _logger.LogInformation("Salary History Payload: {Payload}", JsonSerializer.Serialize(payload));

            var endpoint = string.IsNullOrEmpty(_settings.SalaryHistoryEndpoint)
                ? "/loansvc/data/api/v2/payday/salary/history/provideCustomerDetails"
                : _settings.SalaryHistoryEndpoint;

            var requestUrl = BuildUrl(endpoint);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            AddRemitaHeaders(httpRequest);

            _logger.LogInformation("Sending salary history request to: {Url}", requestUrl);
            _logger.LogInformation("Request headers after AddRemitaHeaders: {Headers}",
                string.Join(", ", httpRequest.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}")));

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<RemitaSalaryHistoryResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Validate Remita response - check multiple indicators of failure
                if (result == null || 
                    result.Status?.ToLower() == "fail" || 
                    result.ResponseCode != "00" || 
                    !result.HasData || 
                    result.Data == null)
                {
                    _logger.LogWarning("Remita returned failure for account {AccountNumber}. Status: {Status}, Code: {Code}, Message: {Message}, HasData: {HasData}",
                        accountNumber, result?.Status, result?.ResponseCode, result?.ResponseMsg, result?.HasData);
                    return null;
                }

                _logger.LogInformation("Successfully retrieved salary history for account: {AccountNumber}", accountNumber);
                return result;
            }
            else
            {
                _logger.LogError("Failed to get salary history. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting salary history for account: {AccountNumber}", accountNumber);
            return null;
        }
    }

    /// <summary>
    /// Creates a loan mandate in Remita
    /// </summary>
    public async Task<RemitaCreateMandateResponseDto?> CreateMandateAsync(
        string customerId,
        string phoneNumber,
        string accountNumber,
        string loanAmount,
        string collectionAmount,
        string dateOfDisbursement,
        string dateOfCollection,
        string totalCollectionAmount,
        string numberOfRepayments,
        string bankCode,
        string? authorisationCode = null
    )
    {
        try
        {
            // Build payload
            var payload = new
            {
                customerId,
                authorisationCode,
                authorisationChannel = "USSD",
                phoneNumber,
                accountNumber,
                currency = "NGN",
                loanAmount,
                collectionAmount,
                dateOfDisbursement,
                dateOfCollection,
                totalCollectionAmount,
                numberOfRepayments,
                bankCode
            };

            _logger.LogInformation("Create Mandate Payload: {Payload}", JsonSerializer.Serialize(payload));

            var endpoint = string.IsNullOrEmpty(_settings.CreateMandateEndpoint)
                ? "/loansvc/data/api/v2/payday/post/loan"
                : _settings.CreateMandateEndpoint;
            var requestUrl = BuildUrl(endpoint);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            AddRemitaHeaders(httpRequest);

            _logger.LogInformation("Sending create mandate request for customer: {CustomerId}", customerId);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("Create Mandate Response - Status: {StatusCode}, Content: {Response}",
                response.StatusCode, responseContent);

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<RemitaCreateMandateResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Validate Remita response - check multiple indicators of failure
                if (result == null || 
                    result.Status?.ToLower() == "fail" || 
                    result.ResponseCode != "00" || 
                    !result.HasData || 
                    result.Data == null)
                {
                    _logger.LogWarning("Remita returned failure for customer {CustomerId}. Status: {Status}, Code: {Code}, Message: {Message}, HasData: {HasData}",
                        customerId, result?.Status, result?.ResponseCode, result?.ResponseMsg, result?.HasData);
                    return null;
                }

                _logger.LogInformation("Successfully created mandate for customer: {CustomerId}, MandateReference: {MandateReference}",
                    customerId, result.Data?.MandateReference);
                return result;
            }
            else
            {
                _logger.LogError("Failed to create mandate. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating mandate for customer: {CustomerId}", customerId);
            return null;
        }
    }

    /// <summary>
    /// Stops an existing loan mandate in Remita
    /// </summary>
    public async Task<RemitaStopMandateResponseDto?> StopMandateAsync(
        string customerId,
        string mandateReference,
        string? authorisationCode)
    {
        try
        {
            // Build payload
            var payload = new
            {
                authorisationCode,
                customerId,
                mandateReference
            };

            var endpoint = string.IsNullOrEmpty(_settings.StopMandateEndpoint)
                ? "/loansvc/data/api/v2/payday/stop/loan"
                : _settings.StopMandateEndpoint;
            var requestUrl = BuildUrl(endpoint);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            AddRemitaHeaders(httpRequest);

            _logger.LogInformation("Sending stop mandate request for customer: {CustomerId}, Mandate: {MandateReference}",
                customerId, mandateReference);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<RemitaStopMandateResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Validate Remita response - check multiple indicators of failure
                if (result == null || 
                    result.Status?.ToLower() == "fail" || 
                    result.Data == null)
                {
                    _logger.LogWarning("Remita returned failure for stopping mandate {MandateReference}. Status: {Status}, Message: {Message}",
                        mandateReference, result?.Status, result?.Data?.Status);
                    return null;
                }

                _logger.LogInformation("Successfully stopped mandate for customer: {CustomerId}", customerId);
                return result;
            }
            else
            {
                _logger.LogError("Failed to stop mandate. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping mandate for customer: {CustomerId}", customerId);
            return null;
        }
    }

    /// <summary>
    /// Gets mandate payment history from Remita
    /// </summary>
    public async Task<RemitaMandateHistoryResponseDto?> GetMandateHistoryAsync(
        string customerId,
        string mandateReference,
        string? authorisationCode)
    {
        try
        {
            // Build payload
            var payload = new
            {
                authorisationCode,
                customerId,
                mandateRef = mandateReference
            };

            var endpoint = string.IsNullOrEmpty(_settings.MandateHistoryEndpoint)
                ? "/loansvc/data/api/v2/payday/loan/payment/history"
                : _settings.MandateHistoryEndpoint;
            var requestUrl = BuildUrl(endpoint);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
            };

            AddRemitaHeaders(httpRequest);

            _logger.LogInformation("Sending mandate history request for customer: {CustomerId}, Mandate: {MandateReference}",
                customerId, mandateReference);

            var response = await _httpClient.SendAsync(httpRequest);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<RemitaMandateHistoryResponseDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                // Validate Remita response - check multiple indicators of failure
                if (result == null || 
                    result.Status?.ToLower() == "fail" || 
                    result.Data == null)
                {
                    _logger.LogWarning("Remita returned failure for mandate history {MandateReference}. Status: {Status}",
                        mandateReference, result?.Status);
                    return null;
                }

                _logger.LogInformation("Successfully retrieved mandate history for customer: {CustomerId}", customerId);
                return result;
            }
            else
            {
                _logger.LogError("Failed to get mandate history. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mandate history for customer: {CustomerId}", customerId);
            return null;
        }
    }

    public Task<BanksResponseDto?> GetBanksAsync()
    {
        throw new NotImplementedException();
    }

    // Private Helper Methods
    private string GenerateRandomAuthorizationCode()
    {
        // Generate a random 5-digit authorization code
        var random = new Random();
        return random.Next(10000, 99999).ToString();
    }

    private string GenerateCustomerId()
    {
        // Generate customer ID using timestamp (like Date.now() in JavaScript)
        return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
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

    private string BuildAuthorizationToken(string requestId)
    {
        var hashInput = $"{_settings.ApiKey}{requestId}{_settings.ApiToken}";
        _logger.LogInformation("Hash Input: ApiKey={ApiKey}, RequestId={RequestId}, ApiToken={ApiToken}",
            _settings.ApiKey, requestId, _settings.ApiToken);
        _logger.LogInformation("Concatenated Hash Input: {HashInput}", hashInput);
        var hash = ComputeSha512Hash(hashInput);
        _logger.LogInformation("Computed Hash: {Hash}", hash);
        return $"remitaConsumerKey={_settings.ApiKey}, remitaConsumerToken={hash}";
    }

    private void AddRemitaHeaders(HttpRequestMessage request)
    {
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Generate dynamic REQUEST_ID as timestamp (like Postman does) - use same value for header and hash
        var requestId = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        _logger.LogInformation("Adding Remita headers - API_KEY: {ApiKey}, MERCHANT_ID: {MerchantId}, REQUEST_ID: {RequestId}",
            _settings.ApiKey, _settings.MerchantId, requestId);

        request.Headers.TryAddWithoutValidation("API_KEY", _settings.ApiKey);
        request.Headers.TryAddWithoutValidation("MERCHANT_ID", _settings.MerchantId);
        request.Headers.TryAddWithoutValidation("REQUEST_ID", requestId);

        // Pass the same requestId to BuildAuthorizationToken
        var authToken = BuildAuthorizationToken(requestId);
        _logger.LogInformation("Generated Authorization Token: {AuthToken}", authToken);

        // Use TryAddWithoutValidation for AUTHORIZATION header because it contains a comma
        var added = request.Headers.TryAddWithoutValidation("AUTHORIZATION", authToken);
        _logger.LogInformation("AUTHORIZATION header added: {Added}", added);

        // Log all headers for debugging
        _logger.LogInformation("All request headers: {Headers}", string.Join(", ", request.Headers.Select(h => $"{h.Key}={string.Join(",", h.Value)}")));
    }

    private static string ComputeSha512Hash(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA512.HashData(bytes);
        return Convert.ToHexStringLower(hash);
    }
}
