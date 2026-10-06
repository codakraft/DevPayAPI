using System.Security.Cryptography;
using System.Text;
using Asp.Versioning;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Application.Exceptions;
using LendingSolution.Core.Settings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LendingSolution.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/webhooks")]
[AllowAnonymous]
public class WebhookController : Controller
{
    private readonly ILoanService _loanService;
    private readonly IMonoService _monoService;
    private readonly MonoSettings _monoSettings;
    private readonly IWalletService _walletService;
    private readonly PaystackSettings _paystackSettings;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        ILoanService loanService,
        IMonoService monoService,
        IOptions<MonoSettings> monoSettings,
        IWalletService walletService,
        IOptions<PaystackSettings> paystackSettings,
        ILogger<WebhookController> logger)
    {
        _loanService = loanService;
        _monoService = monoService;
        _monoSettings = monoSettings.Value;
        _walletService = walletService;
        _paystackSettings = paystackSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Webhook endpoint for Remita loan collection notifications
    /// </summary>
    /// <param name="rawPayload">The raw JSON payload from Remita webhook</param>
    /// <returns>Success response if notification is processed successfully</returns>
    [HttpPost("remita/collection")]
    public async Task<IActionResult> RemitaLoanCollectionNotification([FromBody] System.Text.Json.JsonElement rawPayload)
    {
        try
        {
            // Capture raw JSON payload
            var payloadString = rawPayload.GetRawText();

            // Deserialize to DTO
            var notification = System.Text.Json.JsonSerializer.Deserialize<RemitaLoanCollectionNotificationDto>(payloadString, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("Received Remita loan collection webhook notification - RemitaId: {RemitaId}, MandateRef: {MandateRef}, Amount: {Amount}",
                notification?.Id, notification?.MandateRef, notification?.Amount);

            // Validate the notification
            if (notification == null)
            {
                _logger.LogWarning("Received null notification payload");
                return BadRequest(ApiResponse.Fail("Invalid notification payload"));
            }

            if (string.IsNullOrEmpty(notification.MandateRef))
            {
                _logger.LogWarning("Received notification with empty MandateRef");
                return BadRequest(ApiResponse.Fail("MandateRef is required"));
            }

            // Process the notification using the service
            var result = await _loanService.ProcessLoanCollectionNotificationAsync(notification, payloadString);

            if (result == null)
            {
                _logger.LogError("Failed to process notification for RemitaId: {RemitaId}", notification.Id);
                return StatusCode(500, ApiResponse.Fail("Failed to process notification"));
            }

            _logger.LogInformation("Successfully processed Remita loan collection notification - ID: {Id}, RemitaId: {RemitaId}",
                result.Id, result.RemitaId);

            return Ok(ApiResponse.Ok("Notification received and processed successfully", new
            {
                id = result.Id,
                remitaId = result.RemitaId,
                mandateRef = result.MandateRef,
                amount = result.Amount,
                paymentStatus = result.PaymentStatus,
                processedAt = result.CreatedAt
            }));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Remita loan collection webhook notification");
            return StatusCode(500, ApiResponse.Fail("An error occurred while processing the notification"));
        }
    }

    /// <summary>
    /// Webhook endpoint for Mono direct-debit mandate events.
    /// Register the full public URL of this endpoint
    /// (e.g. https://your-domain.com/api/v1/webhooks/mono) plus the secret on the
    /// Mono dashboard (Apps → Webhooks). Mono then POSTs mandate lifecycle and
    /// debit events here with a <c>mono-webhook-secret</c> header we verify.
    /// </summary>
    [HttpPost("mono")]
    public async Task<IActionResult> MonoWebhook([FromHeader(Name = "mono-webhook-secret")] string? monoSecret)
    {
        try
        {
            // Verify the shared secret before processing anything.
            if (!IsValidMonoSecret(monoSecret))
            {
                _logger.LogWarning("Rejected Mono webhook: missing or invalid mono-webhook-secret header");
                return Unauthorized(ApiResponse.Fail("Invalid webhook signature"));
            }

            // Read the raw body so we can log it and parse per-event data shapes.
            string payloadString;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                payloadString = await reader.ReadToEndAsync();
            }

            var payload = System.Text.Json.JsonSerializer.Deserialize<MonoWebhookPayloadDto>(payloadString, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (payload == null || string.IsNullOrEmpty(payload.Event))
            {
                _logger.LogWarning("Received Mono webhook with empty/invalid payload: {Payload}", payloadString);
                return BadRequest(ApiResponse.Fail("Invalid webhook payload"));
            }

            _logger.LogInformation("Received Mono webhook. Event: {Event}, EventId: {EventId}",
                payload.Event, payload.EventId);

            await _monoService.ProcessWebhookAsync(payload);

            // Always acknowledge quickly with 200 so Mono stops retrying.
            return Ok(ApiResponse.Ok("Webhook received"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Mono webhook notification");
            return StatusCode(500, ApiResponse.Fail("An error occurred while processing the webhook"));
        }
    }

    /// <summary>
    /// Webhook endpoint for Paystack events. Register the full public URL of this endpoint
    /// (e.g. https://your-domain.com/api/v1/webhooks/paystack) on the Paystack dashboard
    /// (Settings → API Keys &amp; Webhooks), separately for test and live mode. Paystack signs each
    /// request with an HMAC-SHA512 of the raw body, keyed with the account's secret key.
    /// <c>charge.success</c> completes the matching wallet funding, so a payment is credited even
    /// if the user never returns to the app. Crediting is idempotent, so retries and the
    /// /payment-success page racing this webhook can't credit twice.
    /// </summary>
    [HttpPost("paystack")]
    public async Task<IActionResult> PaystackWebhook([FromHeader(Name = "x-paystack-signature")] string? signature)
    {
        byte[] body;
        using (var buffer = new MemoryStream())
        {
            await Request.Body.CopyToAsync(buffer);
            body = buffer.ToArray();
        }

        if (!IsValidPaystackSignature(body, signature))
        {
            _logger.LogWarning("Rejected Paystack webhook: missing or invalid x-paystack-signature header");
            return Unauthorized(ApiResponse.Fail("Invalid webhook signature"));
        }

        string? eventName = null;
        string? reference = null;
        try
        {
            using var document = System.Text.Json.JsonDocument.Parse(body);
            var root = document.RootElement;
            if (root.TryGetProperty("event", out var eventElement))
            {
                eventName = eventElement.GetString();
            }
            if (root.TryGetProperty("data", out var data) && data.TryGetProperty("reference", out var referenceElement))
            {
                reference = referenceElement.GetString();
            }
        }
        catch (System.Text.Json.JsonException)
        {
            _logger.LogWarning("Received Paystack webhook with an unreadable body");
            return BadRequest(ApiResponse.Fail("Invalid webhook payload"));
        }

        _logger.LogInformation("Received Paystack webhook. Event: {Event}, Reference: {Reference}", eventName, reference);

        if (eventName != "charge.success" || string.IsNullOrEmpty(reference))
        {
            // Acknowledge events we don't act on so Paystack stops resending them
            return Ok(ApiResponse.Ok("Webhook received"));
        }

        try
        {
            // System completion: no company restriction. It re-verifies with Paystack's API and
            // checks the amount, so it doesn't rely on the webhook body alone.
            var result = await _walletService.CompleteWalletFundingAsync(reference, callerCompanyId: null, userId: null);
            _logger.LogInformation("Paystack webhook completed funding {Reference}: already completed = {AlreadyCompleted}",
                reference, result.AlreadyCompleted);
        }
        catch (AppException ex) when (ex.StatusCode < 500)
        {
            // Not a wallet funding we can complete (unknown reference, pre-status funding, amount
            // mismatch, ...). Retrying won't change that, so acknowledge it.
            _logger.LogWarning("Paystack webhook for {Reference} not applied: {Message}", reference, ex.Message);
        }
        catch (Exception ex)
        {
            // Transient (e.g. Paystack verify or the database unavailable): a non-200 makes Paystack retry
            _logger.LogError(ex, "Error processing Paystack webhook for {Reference}", reference);
            return StatusCode(500, ApiResponse.Fail("An error occurred while processing the webhook"));
        }

        return Ok(ApiResponse.Ok("Webhook received"));
    }

    /// <summary>
    /// Checks Paystack's <c>x-paystack-signature</c>: the hex HMAC-SHA512 of the raw body, keyed
    /// with the secret key. Compared in constant time.
    /// </summary>
    private bool IsValidPaystackSignature(byte[] body, string? signature)
    {
        var secretKey = _paystackSettings.SecretKey;
        if (string.IsNullOrEmpty(secretKey))
        {
            _logger.LogError("Paystack SecretKey is not configured; rejecting webhook.");
            return false;
        }

        if (string.IsNullOrEmpty(signature))
            return false;

        var expected = Convert.ToHexString(HMACSHA512.HashData(Encoding.UTF8.GetBytes(secretKey), body)).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(signature.Trim().ToLowerInvariant()),
            Encoding.UTF8.GetBytes(expected));
    }

    /// <summary>
    /// Constant-time comparison of the incoming <c>mono-webhook-secret</c> header
    /// against the configured secret.
    /// </summary>
    private bool IsValidMonoSecret(string? incomingSecret)
    {
        var expected = _monoSettings.WebhookSecret;

        if (string.IsNullOrEmpty(expected) || expected == "your-webhook-secret-here")
        {
            _logger.LogError("Mono WebhookSecret is not configured; rejecting webhook.");
            return false;
        }

        if (string.IsNullOrEmpty(incomingSecret))
            return false;

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(incomingSecret),
            Encoding.UTF8.GetBytes(expected));
    }
}
