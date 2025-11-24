using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LendingSolution.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MonoController : ControllerBase
{
    private readonly IMonoService _monoService;
    private readonly ILogger<MonoController> _logger;

    public MonoController(IMonoService monoService, ILogger<MonoController> logger)
    {
        _monoService = monoService;
        _logger = logger;
    }

    /// <summary>
    /// Generate mandate using Mono for a loan
    /// </summary>
    /// <param name="loanId">The loan ID to generate mandate for</param>
    /// <param name="request">Mono mandate generation request</param>
    /// <returns>Mono mandate response</returns>
    [HttpPost("generate-mandate/{loanId}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<MonoGenerateMandateResponseDto>> GenerateMandate(
        Guid loanId, 
        [FromBody] MonoGenerateMandateRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            _logger.LogInformation("Generating Mono mandate for loan: {LoanId} by user: {UserId}", loanId, userId);
            
            var result = await _monoService.GenerateMandateAsync(loanId, request, userId);

            if (result == null)
            {
                _logger.LogError("Failed to generate Mono mandate for loan: {LoanId}", loanId);
                return BadRequest(ApiResponse.Fail("Failed to generate mandate with Mono"));
            }

            if (result.Status?.ToLower() == "success")
            {
                _logger.LogInformation("Successfully generated Mono mandate for loan: {LoanId}, MandateId: {MandateId}", 
                    loanId, result.Data?.Id);
                return Ok(ApiResponse.Ok("Mono mandate generated successfully", result));
            }
            else
            {
                _logger.LogWarning("Mono mandate generation failed for loan: {LoanId}, Status: {Status}, Message: {Message}", 
                    loanId, result.Status, result.Message);
                return BadRequest(ApiResponse.Fail(result.Message ?? "Failed to generate mandate"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating Mono mandate for loan: {LoanId}", loanId);
            return StatusCode(500, ApiResponse.Fail("An error occurred while generating the mandate"));
        }
    }

    /// <summary>
    /// Cancel a mandate using Mono
    /// </summary>
    /// <param name="mandateId">The mandate ID to cancel</param>
    /// <returns>Mono cancel mandate response</returns>
    [HttpPatch("cancel-mandate/{mandateId}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<MonoCancelMandateResponseDto>> CancelMandate(string mandateId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            _logger.LogInformation("Cancelling Mono mandate: {MandateId} by user: {UserId}", mandateId, userId);
            
            var result = await _monoService.CancelMandateAsync(mandateId, userId);

            if (result == null)
            {
                _logger.LogError("Failed to cancel Mono mandate: {MandateId}", mandateId);
                return BadRequest(ApiResponse.Fail("Failed to cancel mandate with Mono"));
            }

            if (result.Status?.ToLower() == "success")
            {
                _logger.LogInformation("Successfully cancelled Mono mandate: {MandateId}", mandateId);
                return Ok(ApiResponse.Ok("Mono mandate cancelled successfully", result));
            }
            else
            {
                _logger.LogWarning("Mono mandate cancellation failed for mandate: {MandateId}, Status: {Status}, Message: {Message}", 
                    mandateId, result.Status, result.Message);
                return BadRequest(ApiResponse.Fail(result.Message ?? "Failed to cancel mandate"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling Mono mandate: {MandateId}", mandateId);
            return StatusCode(500, ApiResponse.Fail("An error occurred while cancelling the mandate"));
        }
    }

    /// <summary>
    /// Pause a mandate using Mono
    /// </summary>
    /// <param name="mandateId">The mandate ID to pause</param>
    /// <returns>Mono pause mandate response</returns>
    [HttpPatch("pause-mandate/{mandateId}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<MonoPauseMandateResponseDto>> PauseMandate(string mandateId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            _logger.LogInformation("Pausing Mono mandate: {MandateId} by user: {UserId}", mandateId, userId);
            
            var result = await _monoService.PauseMandateAsync(mandateId, userId);

            if (result == null)
            {
                _logger.LogError("Failed to pause Mono mandate: {MandateId}", mandateId);
                return BadRequest(ApiResponse.Fail("Failed to pause mandate with Mono"));
            }

            if (result.Status?.ToLower() == "success")
            {
                _logger.LogInformation("Successfully paused Mono mandate: {MandateId}", mandateId);
                return Ok(ApiResponse.Ok("Mono mandate paused successfully", result));
            }
            else
            {
                _logger.LogWarning("Mono mandate pause failed for mandate: {MandateId}, Status: {Status}, Message: {Message}", 
                    mandateId, result.Status, result.Message);
                return BadRequest(ApiResponse.Fail(result.Message ?? "Failed to pause mandate"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing Mono mandate: {MandateId}", mandateId);
            return StatusCode(500, ApiResponse.Fail("An error occurred while pausing the mandate"));
        }
    }

    /// <summary>
    /// Reinstate a mandate using Mono
    /// </summary>
    /// <param name="mandateId">The mandate ID to reinstate</param>
    /// <returns>Mono reinstate mandate response</returns>
    [HttpPatch("reinstate-mandate/{mandateId}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<MonoReinstateMandateResponseDto>> ReinstateMandate(string mandateId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            _logger.LogInformation("Reinstating Mono mandate: {MandateId} by user: {UserId}", mandateId, userId);
            
            var result = await _monoService.ReinstateMandateAsync(mandateId, userId);

            if (result == null)
            {
                _logger.LogError("Failed to reinstate Mono mandate: {MandateId}", mandateId);
                return BadRequest(ApiResponse.Fail("Failed to reinstate mandate with Mono"));
            }

            if (result.Status?.ToLower() == "success")
            {
                _logger.LogInformation("Successfully reinstated Mono mandate: {MandateId}", mandateId);
                return Ok(ApiResponse.Ok("Mono mandate reinstated successfully", result));
            }
            else
            {
                _logger.LogWarning("Mono mandate reinstatement failed for mandate: {MandateId}, Status: {Status}, Message: {Message}", 
                    mandateId, result.Status, result.Message);
                return BadRequest(ApiResponse.Fail(result.Message ?? "Failed to reinstate mandate"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reinstating Mono mandate: {MandateId}", mandateId);
            return StatusCode(500, ApiResponse.Fail("An error occurred while reinstating the mandate"));
        }
    }

    /// <summary>
    /// Initiate BVN lookup using Mono
    /// </summary>
    /// <param name="request">BVN lookup request containing BVN and scope</param>
    /// <returns>Mono BVN lookup response</returns>
    [HttpPost("bvn-lookup")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<MonoBvnLookupResponseDto>> BvnLookup([FromBody] MonoBvnLookupRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            _logger.LogInformation("Initiating Mono BVN lookup for BVN: {BVN} by user: {UserId}", request.Bvn, userId);
            
            var result = await _monoService.BvnLookupAsync(request, userId);

            if (result == null)
            {
                _logger.LogError("Failed to initiate BVN lookup for BVN: {BVN}", request.Bvn);
                return BadRequest(ApiResponse.Fail("Failed to initiate BVN lookup with Mono"));
            }

            if (result.Status?.ToLower() == "successful")
            {
                _logger.LogInformation("Successfully initiated BVN lookup for BVN: {BVN}, SessionId: {SessionId}", 
                    request.Bvn, result.Data?.SessionId);
                return Ok(ApiResponse.Ok("BVN lookup initiated successfully", result));
            }
            else
            {
                _logger.LogWarning("Mono BVN lookup failed for BVN: {BVN}, Status: {Status}, Message: {Message}", 
                    request.Bvn, result.Status, result.Message);
                return BadRequest(ApiResponse.Fail(result.Message ?? "Failed to initiate BVN lookup"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating BVN lookup for BVN: {BVN}", request.Bvn);
            return StatusCode(500, ApiResponse.Fail("An error occurred while initiating BVN lookup"));
        }
    }

    /// <summary>
    /// Step 2: Verify BVN lookup with selected method - Parallel to Remita (non-disruptive)
    /// </summary>
    /// <param name="request">BVN verification request containing method and phone number</param>
    /// <returns>Mono BVN verification response</returns>
    [HttpPost("bvn-verify")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<MonoBvnVerifyResponseDto>> BvnVerify([FromBody] MonoBvnVerifyRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            _logger.LogInformation("Processing Mono BVN verification for method: {Method} by user: {UserId}", 
                request.Method, userId);
            
            var result = await _monoService.BvnVerifyAsync(request, userId);

            if (result == null)
            {
                _logger.LogError("Failed to verify BVN with method: {Method}", request.Method);
                return BadRequest(ApiResponse.Fail("Failed to verify BVN with Mono"));
            }

            if (result.Status?.ToLower() == "successful")
            {
                _logger.LogInformation("Successfully initiated BVN verification for method: {Method}", request.Method);
                return Ok(ApiResponse.Ok("BVN verification request sent successfully", result));
            }
            else
            {
                _logger.LogWarning("Mono BVN verification failed for method: {Method}, Status: {Status}, Message: {Message}", 
                    request.Method, result.Status, result.Message);
                return BadRequest(ApiResponse.Fail(result.Message ?? "Failed to verify BVN"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying BVN for method: {Method}", request.Method);
            return StatusCode(500, ApiResponse.Fail("An error occurred while verifying BVN"));
        }
    }

    /// <summary>
    /// Step 3: Get BVN details using OTP - Parallel to Remita (non-disruptive)
    /// </summary>
    /// <param name="request">BVN details request containing OTP</param>
    /// <returns>Mono BVN details response</returns>
    [HttpPost("bvn-details")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<MonoBvnDetailsResponseDto>> BvnGetDetails([FromBody] MonoBvnDetailsRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            _logger.LogInformation("Retrieving Mono BVN details with OTP by user: {UserId}", userId);
            
            var result = await _monoService.BvnGetDetailsAsync(request, userId);

            if (result == null)
            {
                _logger.LogError("Failed to get BVN details from Mono");
                return BadRequest(ApiResponse.Fail("Failed to get BVN details from Mono"));
            }

            if (result.Status?.ToLower() == "successful")
            {
                _logger.LogInformation("Successfully retrieved BVN details for BVN: {BVN}", 
                    result.Data?.Bvn ?? "unknown");
                return Ok(ApiResponse.Ok("BVN details retrieved successfully", result));
            }
            else
            {
                _logger.LogWarning("Mono BVN details retrieval failed: Status: {Status}, Message: {Message}", 
                    result.Status, result.Message);
                return BadRequest(ApiResponse.Fail(result.Message ?? "Failed to get BVN details"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting BVN details");
            return StatusCode(500, ApiResponse.Fail("An error occurred while getting BVN details"));
        }
    }

    /// <summary>
    /// Complete BVN validation workflow (all steps in one) - Parallel to Remita (non-disruptive)
    /// </summary>
    /// <param name="request">Complete BVN validation request</param>
    /// <returns>Mono BVN validation result</returns>
    [HttpPost("bvn-validate-complete")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<MonoBvnValidationResultDto>> BvnValidateComplete([FromBody] MonoBvnCompleteValidationRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            _logger.LogInformation("Starting complete Mono BVN validation for BVN: {BVN} by user: {UserId}", 
                request.Bvn, userId);
            
            var result = await _monoService.ValidateBvnCompleteAsync(
                request.Bvn, 
                request.Method, 
                request.PhoneNumber, 
                request.Otp, 
                request.Scope, 
                userId);

            if (result == null)
            {
                _logger.LogError("Failed to complete BVN validation for BVN: {BVN}", request.Bvn);
                return BadRequest(ApiResponse.Fail("Failed to complete BVN validation"));
            }

            if (result.IsValidated)
            {
                _logger.LogInformation("Successfully completed BVN validation for BVN: {BVN}, SessionId: {SessionId}", 
                    request.Bvn, result.SessionId);
                return Ok(ApiResponse.Ok("BVN validation completed successfully", result));
            }
            else
            {
                _logger.LogWarning("BVN validation failed for BVN: {BVN}, Message: {Message}", 
                    request.Bvn, result.ValidationMessage);
                return BadRequest(ApiResponse.Fail(result.ValidationMessage));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in complete BVN validation for BVN: {BVN}", request.Bvn);
            return StatusCode(500, ApiResponse.Fail("An error occurred during BVN validation"));
        }
    }

    // ======================== Credit History & Analysis Endpoints ========================

    /// <summary>
    /// Get credit history for a BVN using Mono credit history API
    /// </summary>
    /// <param name="request">Credit history request containing BVN and provider</param>
    /// <returns>Credit history response from Mono</returns>
    [HttpPost("credit-history")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<MonoCreditHistoryResponseDto>> GetCreditHistory([FromBody] MonoCreditHistoryRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            _logger.LogInformation("Getting credit history for BVN: {BvnMasked} with provider: {Provider} by user: {UserId}", 
                MaskBvn(request.Bvn), request.Provider, userId);
            
            var result = await _monoService.GetCreditHistoryAsync(request.Bvn, request.Provider, userId);

            if (result == null)
            {
                _logger.LogError("Failed to get credit history for BVN: {BvnMasked}", MaskBvn(request.Bvn));
                return BadRequest(ApiResponse.Fail("Failed to retrieve credit history from Mono"));
            }

            if (result.Status?.ToLower() == "successful")
            {
                _logger.LogInformation("Successfully retrieved credit history for BVN: {BvnMasked} with provider: {Provider}", 
                    MaskBvn(request.Bvn), request.Provider);
                return Ok(ApiResponse.Ok("Credit history retrieved successfully", result));
            }
            else
            {
                _logger.LogWarning("Credit history retrieval failed for BVN: {BvnMasked}, Status: {Status}, Message: {Message}", 
                    MaskBvn(request.Bvn), result.Status, result.Message);
                return BadRequest(ApiResponse.Fail(result.Message ?? "Failed to retrieve credit history"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting credit history for BVN: {BvnMasked}", MaskBvn(request.Bvn));
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving credit history"));
        }
    }

    /// <summary>
    /// Get comprehensive credit analysis for a BVN (includes credit history and risk assessment)
    /// </summary>
    /// <param name="request">Credit analysis request containing BVN and provider</param>
    /// <returns>Complete credit analysis with risk assessment and loan recommendations</returns>
    [HttpPost("credit-analysis")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<MonoCreditAnalysisResultDto>> GetCreditAnalysis([FromBody] MonoCreditHistoryRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            _logger.LogInformation("Performing credit analysis for BVN: {BvnMasked} with provider: {Provider} by user: {UserId}", 
                MaskBvn(request.Bvn), request.Provider, userId);
            
            var result = await _monoService.AnalyzeCreditHistoryAsync(request.Bvn, request.Provider, userId);

            if (result == null)
            {
                _logger.LogError("Failed to analyze credit history for BVN: {BvnMasked}", MaskBvn(request.Bvn));
                return BadRequest(ApiResponse.Fail("Failed to analyze credit history"));
            }

            _logger.LogInformation("Successfully completed credit analysis for BVN: {BvnMasked} - Score: {Score}, Risk: {Risk}, Action: {Action}", 
                MaskBvn(request.Bvn), result.CreditScore, result.RiskLevel, result.RecommendedAction);
            
            return Ok(ApiResponse.Ok("Credit analysis completed successfully", result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing credit history for BVN: {BvnMasked}", MaskBvn(request.Bvn));
            return StatusCode(500, ApiResponse.Fail("An error occurred during credit analysis"));
        }
    }

    // ======================== Session-based BVN Validation Endpoints ========================

    /// <summary>
    /// Start BVN verification with session support
    /// </summary>
    /// <param name="request">Session-based BVN verification request</param>
    /// <returns>BVN verification response with session ID</returns>
    [HttpPost("bvn/verify-with-session")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<MonoBvnVerifyResponseDto>> BvnVerifyWithSession([FromBody] MonoBvnSessionRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            _logger.LogInformation("Starting session-based BVN verification via {Method} by user: {UserId}", 
                request.Method, userId);
            
            var result = await _monoService.BvnVerifyWithSessionAsync(request, userId);

            if (result == null)
            {
                _logger.LogError("Failed to start session-based BVN verification");
                return BadRequest(ApiResponse.Fail("Failed to start BVN verification"));
            }

            if (result.Status?.ToLower() == "successful")
            {
                _logger.LogInformation("Successfully started session-based BVN verification, Data: {Data}", result.Data);
                return Ok(ApiResponse.Ok("BVN verification started successfully", result));
            }
            else
            {
                _logger.LogWarning("Session-based BVN verification failed, Status: {Status}, Message: {Message}", 
                    result.Status, result.Message);
                return BadRequest(ApiResponse.Fail(result.Message ?? "BVN verification failed"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in session-based BVN verification");
            return StatusCode(500, ApiResponse.Fail("An error occurred during BVN verification"));
        }
    }

    /// <summary>
    /// Complete BVN verification with session support
    /// </summary>
    /// <param name="request">Session-based BVN details request with OTP</param>
    /// <returns>Complete BVN details response</returns>
    [HttpPost("bvn/details-with-session")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<MonoBvnDetailsResponseDto>> BvnGetDetailsWithSession([FromBody] MonoBvnSessionDetailsRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            _logger.LogInformation("Completing session-based BVN verification with OTP by user: {UserId}", userId);
            
            var result = await _monoService.BvnGetDetailsWithSessionAsync(request, userId);

            if (result == null)
            {
                _logger.LogError("Failed to complete session-based BVN verification");
                return BadRequest(ApiResponse.Fail("Failed to get BVN details"));
            }

            if (result.Status?.ToLower() == "successful")
            {
                _logger.LogInformation("Successfully completed session-based BVN verification for BVN: {BvnMasked}", 
                    MaskBvn(result.Data?.Bvn ?? ""));
                return Ok(ApiResponse.Ok("BVN details retrieved successfully", result));
            }
            else
            {
                _logger.LogWarning("Session-based BVN details retrieval failed, Status: {Status}, Message: {Message}", 
                    result.Status, result.Message);
                return BadRequest(ApiResponse.Fail(result.Message ?? "Failed to get BVN details"));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in session-based BVN details retrieval");
            return StatusCode(500, ApiResponse.Fail("An error occurred while getting BVN details"));
        }
    }

    // ======================== BVN Verification Tracking Endpoints ========================

    /// <summary>
    /// Check if a BVN has already been verified and is still valid
    /// </summary>
    /// <param name="bvn">The BVN to check</param>
    /// <returns>Boolean indicating if BVN is already verified</returns>
    [HttpGet("bvn/is-verified/{bvn}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<bool>> IsBvnAlreadyVerified(string bvn)
    {
        try
        {
            _logger.LogInformation("Checking verification status for BVN: {BvnMasked}", MaskBvn(bvn));
            
            var isVerified = await _monoService.IsBvnAlreadyVerifiedAsync(bvn);
            
            _logger.LogInformation("BVN verification status for {BvnMasked}: {IsVerified}", MaskBvn(bvn), isVerified);
            
            return Ok(ApiResponse.Ok("BVN verification status retrieved", isVerified));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking BVN verification status for BVN: {BvnMasked}", MaskBvn(bvn));
            return StatusCode(500, ApiResponse.Fail("An error occurred while checking BVN verification status"));
        }
    }

    /// <summary>
    /// Get BVN verification record details
    /// </summary>
    /// <param name="bvn">The BVN to get verification record for</param>
    /// <returns>BVN verification record details</returns>
    [HttpGet("bvn/verification-record/{bvn}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<MonoBvnVerificationRecord>> GetBvnVerificationRecord(string bvn)
    {
        try
        {
            _logger.LogInformation("Getting verification record for BVN: {BvnMasked}", MaskBvn(bvn));
            
            var record = await _monoService.GetBvnVerificationRecordAsync(bvn);
            
            if (record == null)
            {
                _logger.LogInformation("No verification record found for BVN: {BvnMasked}", MaskBvn(bvn));
                return NotFound(ApiResponse.Fail("No verification record found for this BVN"));
            }
            
            _logger.LogInformation("Found verification record for BVN: {BvnMasked}, Verified: {IsVerified}", 
                MaskBvn(bvn), record.IsVerified);
            
            return Ok(ApiResponse.Ok("BVN verification record retrieved", record));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting BVN verification record for BVN: {BvnMasked}", MaskBvn(bvn));
            return StatusCode(500, ApiResponse.Fail("An error occurred while getting BVN verification record"));
        }
    }

    // ======================== Private Helper Methods ========================

    private static string MaskBvn(string bvn)
    {
        if (string.IsNullOrEmpty(bvn) || bvn.Length < 4)
            return "****";
        
        return $"{bvn[..3]}***{bvn[^1]}";
    }

    /// <summary>
    /// Get Mono service status (for health checks)
    /// </summary>
    /// <returns>Service status</returns>
    [HttpGet("status")]
    [AllowAnonymous]
    public IActionResult GetStatus()
    {
        return Ok(ApiResponse.Ok("Mono service is available", new { 
            Service = "Mono", 
            Version = "v3", 
            Timestamp = DateTime.UtcNow 
        }));
    }
}