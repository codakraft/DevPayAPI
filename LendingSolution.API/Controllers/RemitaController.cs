using Asp.Versioning;
using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LendingSolution.API.Controllers;

/// <summary>
/// Remita Direct Debit Mandate API
/// Provides endpoints to create, activate (via OTP) and cancel Direct Debit mandates
/// using Remita's echannelsvc/echannel/mandate API.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/remita/direct-debit")]
[Authorize]
public class RemitaController(
    IRemitaService remitaService,
    ILogger<RemitaController> logger
) : ControllerBase
{
    private readonly IRemitaService _remitaService = remitaService;
    private readonly ILogger<RemitaController> _logger = logger;

    /// <summary>
    /// Step 1 – Creates a new Direct Debit mandate in Remita.
    /// On success, returns a mandateId that must be passed to the activation steps.
    /// </summary>
    [HttpPost("mandate/generate")]
    public async Task<IActionResult> GenerateMandate([FromBody] DirectDebitGenerateMandateRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogInformation("[RemitaController] GenerateMandate called for payer: {PayerName}", request.PayerName);

            var result = await _remitaService.GenerateDirectDebitMandateAsync(request);

            if (result == null)
                return StatusCode(502, new { message = "No response received from Remita. Please try again." });

            if (result.StatusCode != "00")
                return BadRequest(new { message = result.Message ?? "Remita returned a failure response.", remitaStatusCode = result.StatusCode });

            return Ok(new
            {
                message    = "Mandate created successfully. Proceed to activate with OTP.",
                mandateId  = result.Data?.MandateId,
                requestId  = result.Data?.RequestId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RemitaController] GenerateMandate threw an unhandled exception");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Step 2a – Triggers an OTP to be sent to the mandate holder's phone.
    /// Call this after creating the mandate to initiate activation.
    /// </summary>
    [HttpPost("mandate/request-otp")]
    public async Task<IActionResult> RequestActivationOtp([FromBody] DirectDebitRequestAuthorizationDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogInformation("[RemitaController] RequestActivationOtp for MandateId: {MandateId}", request.MandateId);

            var result = await _remitaService.RequestMandateAuthorizationAsync(request);

            if (result == null)
                return StatusCode(502, new { message = "No response received from Remita. Please try again." });

            if (result.StatusCode != "00")
                return BadRequest(new { message = result.Message ?? "Failed to dispatch OTP.", remitaStatusCode = result.StatusCode });

            return Ok(new { message = "OTP sent successfully. Ask the customer to check their phone.", requestId = result.RequestId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RemitaController] RequestActivationOtp threw an unhandled exception");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Step 2b – Validates the OTP entered by the customer, completing mandate activation.
    /// A successful response means the Direct Debit mandate is now active.
    /// </summary>
    [HttpPost("mandate/validate-otp")]
    public async Task<IActionResult> ValidateActivationOtp([FromBody] DirectDebitValidateAuthorizationDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogInformation("[RemitaController] ValidateActivationOtp for MandateId: {MandateId}", request.MandateId);

            var result = await _remitaService.ValidateMandateAuthorizationAsync(request);

            if (result == null)
                return StatusCode(502, new { message = "No response received from Remita. Please try again." });

            if (result.StatusCode != "00")
                return BadRequest(new { message = result.Message ?? "OTP validation failed.", remitaStatusCode = result.StatusCode });

            return Ok(new
            {
                message    = "Mandate activated successfully.",
                mandateRef = result.MandateRef
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RemitaController] ValidateActivationOtp threw an unhandled exception");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }

    /// <summary>
    /// Cancels an active Direct Debit mandate.
    /// </summary>
    [HttpPost("mandate/stop")]
    public async Task<IActionResult> StopMandate([FromBody] DirectDebitStopMandateRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogInformation("[RemitaController] StopMandate for MandateId: {MandateId}", request.MandateId);

            var result = await _remitaService.StopDirectDebitMandateAsync(request);

            if (result == null)
                return StatusCode(502, new { message = "No response received from Remita. Please try again." });

            if (result.StatusCode != "00")
                return BadRequest(new { message = result.Message ?? "Failed to stop mandate.", remitaStatusCode = result.StatusCode });

            return Ok(new { message = "Mandate stopped successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RemitaController] StopMandate threw an unhandled exception");
            return StatusCode(500, new { message = "An unexpected error occurred." });
        }
    }
}
