using System.Security.Cryptography;
using System.Text;
using Asp.Versioning;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
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
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        ILoanService loanService,
        IMonoService monoService,
        IOptions<MonoSettings> monoSettings,
        ILogger<WebhookController> logger)
    {
        _loanService = loanService;
        _monoService = monoService;
        _monoSettings = monoSettings.Value;
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
