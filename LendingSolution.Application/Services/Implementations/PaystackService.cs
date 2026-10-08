using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LendingSolution.Application.Services.Implementations;

/// <summary>
/// Paystack service implementation for payment operations
/// </summary>
public class PaystackService : IPaystackService
{
    private readonly HttpClient _httpClient;
    private readonly PaystackSettings _paystackSettings;
    private readonly ILogger<PaystackService> _logger;

    public PaystackService(HttpClient httpClient, IOptions<PaystackSettings> paystackSettings, ILogger<PaystackService> logger)
    {
        _httpClient = httpClient;
        _paystackSettings = paystackSettings.Value;
        _logger = logger;

        // Configure HTTP client
        _httpClient.BaseAddress = new Uri(_paystackSettings.BaseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _paystackSettings.SecretKey);
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<PaystackInitializationDto> InitializePaymentAsync(string email, decimal amount, string reference, string? callbackUrl = null)
    {
        try
        {
            var redirectUrl = callbackUrl ?? _paystackSettings.CallbackUrl;
            var payload = new Dictionary<string, object?>
            {
                ["email"] = email,
                ["amount"] = (int)(amount * 100), // Paystack expects amount in kobo (for NGN)
                ["reference"] = reference,
                ["callback_url"] = redirectUrl
            };

            // Paystack only follows callback_url after a completed payment; cancel_action is where its
            // checkout sends the user when they cancel or the payment is declined
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                payload["metadata"] = new Dictionary<string, string>
                {
                    ["cancel_action"] = $"{redirectUrl}{(redirectUrl.Contains('?') ? '&' : '?')}status=cancelled"
                };
            }

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _logger.LogInformation("Initializing Paystack payment for email: {Email}, amount: {Amount}, reference: {Reference}", 
                email, amount, reference);

            var response = await _httpClient.PostAsync("/transaction/initialize", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<PaystackInitializationDto>(responseContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
                });

                _logger.LogInformation("Paystack payment initialization successful for reference: {Reference}", reference);
                return result ?? new PaystackInitializationDto { Status = false, Message = "Invalid response from Paystack" };
            }
            else
            {
                _logger.LogError("Paystack payment initialization failed. Status: {Status}, Response: {Response}", 
                    response.StatusCode, responseContent);
                
                return new PaystackInitializationDto
                {
                    Status = false,
                    Message = $"Payment initialization failed: {response.StatusCode}"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing Paystack payment for reference: {Reference}", reference);
            return new PaystackInitializationDto
            {
                Status = false,
                Message = "An error occurred while initializing payment"
            };
        }
    }

    public async Task<PaystackVerificationResult> VerifyTransactionAsync(string reference)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/transaction/verify/{Uri.EscapeDataString(reference)}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Paystack verify failed for reference {Reference}: {Status} {Response}",
                    reference, response.StatusCode, responseContent);
                return new PaystackVerificationResult { Verified = false };
            }

            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;
            if (!root.TryGetProperty("status", out var okElement) || !okElement.GetBoolean() ||
                !root.TryGetProperty("data", out var data))
            {
                return new PaystackVerificationResult { Verified = false };
            }

            var result = new PaystackVerificationResult
            {
                Verified = true,
                Status = data.TryGetProperty("status", out var status) ? status.GetString() : null,
                AmountKobo = data.TryGetProperty("amount", out var amount) && amount.ValueKind == JsonValueKind.Number
                    ? amount.GetInt64()
                    : 0,
                Currency = data.TryGetProperty("currency", out var currency) ? currency.GetString() : null
            };

            _logger.LogInformation("Paystack verify for reference {Reference}: {Status}, {Amount} {Currency}",
                reference, result.Status, result.AmountKobo, result.Currency);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Paystack transaction {Reference}", reference);
            return new PaystackVerificationResult { Verified = false };
        }
    }

    public async Task<bool> VerifyPaymentAsync(string reference)
    {
        try
        {
            _logger.LogInformation("Verifying Paystack payment for reference: {Reference}", reference);

            var response = await _httpClient.GetAsync($"/transaction/verify/{reference}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var document = JsonDocument.Parse(responseContent);
                var root = document.RootElement;
                
                if (root.TryGetProperty("status", out var statusElement) && statusElement.GetBoolean())
                {
                    if (root.TryGetProperty("data", out var dataElement) &&
                        dataElement.TryGetProperty("status", out var paymentStatusElement))
                    {
                        var paymentStatus = paymentStatusElement.GetString();
                        var isSuccess = paymentStatus?.Equals("success", StringComparison.OrdinalIgnoreCase) == true;
                        
                        _logger.LogInformation("Paystack payment verification result for reference {Reference}: {Status}", 
                            reference, paymentStatus);
                        
                        return isSuccess;
                    }
                }
            }

            _logger.LogWarning("Paystack payment verification failed for reference: {Reference}. Response: {Response}", 
                reference, responseContent);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying Paystack payment for reference: {Reference}", reference);
            return false;
        }
    }

    public async Task<dynamic?> GetPaymentDetailsAsync(string reference)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/transaction/verify/{reference}");
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                using var document = JsonDocument.Parse(responseContent);
                return document.RootElement;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Paystack payment details for reference: {Reference}", reference);
            return null;
        }
    }
}
