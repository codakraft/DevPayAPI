using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;
using LendingSolution.Core.Models;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using LendingSolution.API.Auth;
using LendingSolution.Core.Auth;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LendingSolution.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class MonoController : ControllerBase
{
    private readonly IMonoService _monoService;
    private readonly ILoanService _loanService;
    private readonly ILogger<MonoController> _logger;

    public MonoController(IMonoService monoService, ILoanService loanService, ILogger<MonoController> logger)
    {
        _monoService = monoService;
        _loanService = loanService;
        _logger = logger;
    }

    /// <summary>
    /// Create a Mono customer
    /// </summary>
    /// <param name="request">Customer details</param>
    /// <returns>Created Mono customer with ID</returns>
    [HttpPost("customers")]
    [HasPermission(Permissions.Loans.Approve)]
    public async Task<ActionResult<MonoCreateCustomerResponseDto>> CreateCustomer([FromBody] MonoCreateCustomerRequestDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("Creating Mono customer for email: {Email} by user: {UserId}", request.Email, userId);

            var result = await _monoService.CreateCustomerAsync(request, userId);

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to create Mono customer"));

            if (result.Data != null)
            {
                _logger.LogInformation("Mono customer created successfully. Id: {CustomerId}", result.Data.Id);
                return StatusCode(201, ApiResponse.Ok("Mono customer created successfully", result));
            }

            _logger.LogWarning("Mono customer creation returned no data. Message: {Message}", result.Message);
            return BadRequest(ApiResponse.Fail(result.Message ?? "Failed to create Mono customer"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Mono customer for email: {Email}", request.Email);
            return StatusCode(500, ApiResponse.Fail("An error occurred while creating the customer"));
        }
    }

    /// <summary>
    /// Get a Mono customer by ID
    /// </summary>
    /// <param name="customerId">The Mono customer ID</param>
    /// <returns>Customer details</returns>
    [HttpGet("customers/{customerId}")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<MonoGetCustomerResponseDto>> GetCustomerById(string customerId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("Fetching Mono customer {CustomerId} by user: {UserId}", customerId, userId);

            var result = await _monoService.GetCustomerByIdAsync(customerId, userId);

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to retrieve Mono customer"));

            if (result.Data != null)
                return Ok(ApiResponse.Ok("Mono customer retrieved successfully", result));

            return BadRequest(ApiResponse.Fail(result.Message ?? "Customer not found"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Mono customer: {CustomerId}", customerId);
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving the customer"));
        }
    }

    /// <summary>
    /// Get all Mono customers
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="limit">Page size (default: 20)</param>
    /// <returns>Paginated list of customers</returns>
    [HttpGet("customers")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<MonoGetAllCustomersResponseDto>> GetAllCustomers(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("Fetching all Mono customers. Page: {Page}, Limit: {Limit}, by user: {UserId}", page, limit, userId);

            var result = await _monoService.GetAllCustomersAsync(page, limit, userId);

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to retrieve Mono customers"));

            return Ok(ApiResponse.Ok("Mono customers retrieved successfully", result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all Mono customers");
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving customers"));
        }
    }

    /// <summary>
    /// Get all linked accounts for a Mono customer
    /// </summary>
    /// <param name="customerId">The Mono customer ID</param>
    /// <returns>List of linked bank accounts</returns>
    [HttpGet("customers/{customerId}/accounts")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<MonoGetLinkedAccountsResponseDto>> GetCustomerLinkedAccounts(string customerId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("Fetching linked accounts for Mono customer: {CustomerId} by user: {UserId}", customerId, userId);

            var result = await _monoService.GetCustomerLinkedAccountsAsync(customerId, userId);

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to retrieve linked accounts"));

            return Ok(ApiResponse.Ok("Linked accounts retrieved successfully", result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching linked accounts for Mono customer: {CustomerId}", customerId);
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving linked accounts"));
        }
    }

    /// <summary>
    /// Generate mandate using Mono for a loan
    /// </summary>
    /// <param name="loanId">The loan ID to generate mandate for</param>
    /// <param name="request">Mono mandate generation request</param>
    /// <returns>Mono mandate response</returns>
    [HttpPost("generate-mandate/{loanId}")]
    [HasPermission(Permissions.Collections.Manage)]
    public async Task<ActionResult<MonoGenerateMandateResponseDto>> GenerateMandate(
        Guid loanId, 
        [FromBody] MonoGenerateMandateRequestDto request)
    {
        try
        {
            if (!await User.CanAccessLoanAsync(_loanService, loanId))
            {
                return NotFound(ApiResponse.Fail("Loan not found"));
            }

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
    [HasPermission(Permissions.Collections.Manage)]
    public async Task<ActionResult<MonoCancelMandateResponseDto>> CancelMandate(string mandateId)
    {
        try
        {
            if (!await User.CanAccessMonoMandateAsync(_monoService, mandateId))
            {
                return NotFound(ApiResponse.Fail("Mandate not found"));
            }

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
    [HasPermission(Permissions.Collections.Manage)]
    public async Task<ActionResult<MonoPauseMandateResponseDto>> PauseMandate(string mandateId)
    {
        try
        {
            if (!await User.CanAccessMonoMandateAsync(_monoService, mandateId))
            {
                return NotFound(ApiResponse.Fail("Mandate not found"));
            }

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
    [HasPermission(Permissions.Collections.Manage)]
    public async Task<ActionResult<MonoReinstateMandateResponseDto>> ReinstateMandate(string mandateId)
    {
        try
        {
            if (!await User.CanAccessMonoMandateAsync(_monoService, mandateId))
            {
                return NotFound(ApiResponse.Fail("Mandate not found"));
            }

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
    /// Lookup NIN using Mono
    /// </summary>
    /// <param name="nin">The National Identification Number to verify</param>
    /// <returns>NIN details from Mono</returns>
    [HttpPost("nin-lookup")]
    [HasPermission(Permissions.Loans.Approve)]
    public async Task<ActionResult<MonoNinLookupResponseDto>> NinLookup([FromQuery] string nin)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("NIN lookup requested for NIN: {Nin} by user: {UserId}", nin, userId);

            if (string.IsNullOrWhiteSpace(nin))
                return BadRequest(ApiResponse.Fail("NIN is required"));

            var result = await _monoService.NinLookupAsync(nin, userId);

            if (result == null)
                return BadRequest(ApiResponse.Fail("NIN lookup failed"));

            if (result.Data != null)
                return Ok(ApiResponse.Ok("NIN lookup successful", result));

            return BadRequest(ApiResponse.Fail(result.Message ?? "NIN lookup returned no data"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing NIN lookup for NIN: {Nin}", nin);
            return StatusCode(500, ApiResponse.Fail("An error occurred during NIN lookup"));
        }
    }

    /// <summary>
    /// Initiate BVN lookup using Mono
    /// </summary>
    /// <param name="request">BVN lookup request containing BVN and scope</param>
    /// <returns>Mono BVN lookup response</returns>
    [HttpPost("bvn-lookup")]
    [HasPermission(Permissions.Loans.Approve)]
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
    /// <param name="sessionId">Mono session ID from initiate step</param>
    /// <returns>Mono BVN verification response</returns>
    [HttpPost("bvn-verify")]
    [HasPermission(Permissions.Loans.Approve)]
    public async Task<ActionResult<MonoBvnVerifyResponseDto>> BvnVerify([FromBody] MonoBvnVerifyRequestDto request, [FromHeader(Name = "x-session-id")] string sessionId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            _logger.LogInformation("Processing Mono BVN verification for method: {Method} by user: {UserId}", 
                request.Method, userId);
            
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest(ApiResponse.Fail("x-session-id header is required for BVN verification"));
            }
            
            var result = await _monoService.BvnVerifyAsync(request, sessionId, userId);

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
    /// <param name="sessionId">Mono session ID from initiate step</param>
    /// <returns>Mono BVN details response</returns>
    [HttpPost("bvn-details")]
    [HasPermission(Permissions.Loans.Approve)]
    public async Task<ActionResult<MonoBvnDetailsResponseDto>> BvnGetDetails([FromBody] MonoBvnDetailsRequestDto request, [FromHeader(Name = "x-session-id")] string sessionId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            _logger.LogInformation("Retrieving Mono BVN details with OTP by user: {UserId}", userId);
            
            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest(ApiResponse.Fail("x-session-id header is required for BVN details retrieval"));
            }
            
            var result = await _monoService.BvnGetDetailsAsync(request, sessionId, userId);

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
    [HasPermission(Permissions.Loans.Approve)]
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
    [HasPermission(Permissions.Loans.Approve)]
    public async Task<ActionResult<MonoCreditHistoryResponseDto>> GetCreditHistory([FromBody] MonoCreditHistoryRequestDto request, [FromQuery] string provider = "xds")
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("Getting credit history for BVN: {BvnMasked} with provider: {Provider} by user: {UserId}",
                MaskBvn(request.Bvn), provider, userId);

            var result = await _monoService.GetCreditHistoryAsync(request.Bvn, provider, userId);

            if (result == null)
            {
                _logger.LogError("Failed to get credit history for BVN: {BvnMasked}", MaskBvn(request.Bvn));
                return BadRequest(ApiResponse.Fail("Failed to retrieve credit history from Mono"));
            }

            if (result.Status?.ToLower() == "successful")
            {
                _logger.LogInformation("Successfully retrieved credit history for BVN: {BvnMasked} with provider: {Provider}",
                    MaskBvn(request.Bvn), provider);
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
    [HasPermission(Permissions.Loans.Approve)]
    public async Task<ActionResult<MonoCreditAnalysisResultDto>> GetCreditAnalysis([FromBody] MonoCreditHistoryRequestDto request, [FromQuery] string provider = "xds")
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("Performing credit analysis for BVN: {BvnMasked} with provider: {Provider} by user: {UserId}",
                MaskBvn(request.Bvn), provider, userId);

            var result = await _monoService.AnalyzeCreditHistoryAsync(request.Bvn, provider, userId);

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
    [HasPermission(Permissions.Loans.Approve)]
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
    [HasPermission(Permissions.Loans.Approve)]
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
    [HasPermission(Permissions.Loans.Approve)]
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
    [HasPermission(Permissions.Loans.Approve)]
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

    // ======================== Banks Endpoint ========================

    /// <summary>
    /// Returns the list of banks supported by Mono for mandate creation.
    /// </summary>
    [HttpGet("banks")]
    [AllowAnonymous]
    public async Task<IActionResult> GetBanks()
    {
        try
        {
            var result = await _monoService.GetBanksAsync();

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to retrieve banks from Mono"));

            result.Data = result.Data.Where(b => b.DirectDebit).ToList();

            return Ok(ApiResponse.Ok("Banks retrieved successfully", result));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Mono banks list");
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving banks"));
        }
    }

    // ======================== Initiate Debit Endpoint ========================

    /// <summary>
    /// Manually triggers a one-off debit collection against an active mandate.
    /// For <c>variable</c> mandates you can supply a custom <c>amount</c> (in kobo).
    /// For <c>fixed</c> mandates omit <c>amount</c> and Mono uses the mandate amount.
    /// </summary>
    /// <param name="mandateId">The Mono mandate ID to debit</param>
    /// <param name="request">Optional amount (kobo) and description</param>
    [HttpPost("mandate/{mandateId}/debit")]
    [HasPermission(Permissions.Collections.Manage)]
    public async Task<IActionResult> InitiateDebit(string mandateId, [FromBody] MonoInitiateDebitRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            if (!await User.CanAccessMonoMandateAsync(_monoService, mandateId))
            {
                return NotFound(ApiResponse.Fail("Mandate not found"));
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            _logger.LogInformation("InitiateDebit called for MandateId={MandateId}, Amount={Amount}, UserId={UserId}",
                mandateId, request.Amount, userId);

            var result = await _monoService.InitiateDebitAsync(mandateId, request, userId);

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to initiate debit. Check that the mandate is active and ready to debit."));

            if (result.Status?.ToLower() == "success" || result.Status?.ToLower() == "successful")
                return Ok(ApiResponse.Ok("Debit initiated successfully", result));

            return BadRequest(ApiResponse.Fail(result.Message ?? "Mono returned a non-success status"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initiating debit for mandate {MandateId}", mandateId);
            return StatusCode(500, ApiResponse.Fail("An unexpected error occurred while initiating the debit"));
        }
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