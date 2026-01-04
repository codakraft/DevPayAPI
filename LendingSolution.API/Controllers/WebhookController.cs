using Asp.Versioning;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/webhooks")]
public class WebhookController : Controller
{
    private readonly ILoanService _loanService;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        ILoanService loanService,
        ILogger<WebhookController> logger)
    {
        _loanService = loanService;
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
}
