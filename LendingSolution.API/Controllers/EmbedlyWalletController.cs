using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.Core.Dtos.Response;

namespace LendingSolution.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/wallets")]
public class EmbedlyWalletController : ControllerBase
{
    private readonly IEmbedlyWalletService _embedlyWalletService;
    private readonly ILogger<EmbedlyWalletController> _logger;

    public EmbedlyWalletController(IEmbedlyWalletService embedlyWalletService, ILogger<EmbedlyWalletController> logger)
    {
        _embedlyWalletService = embedlyWalletService;
        _logger = logger;
    }

    // ─── Customer Endpoints ───────────────────────────────────────────────────

    /// <summary>
    /// Create a new Embedly customer
    /// </summary>
    [HttpPost("customers")]
    public async Task<IActionResult> CreateCustomer([FromBody] EmbedlyCreateCustomerRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogInformation("Creating Embedly customer for Email={Email}", request.EmailAddress);

            var result = await _embedlyWalletService.CreateCustomerAsync(request);

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to create customer on Embedly"));

            if (result.Status)
                return Ok(ApiResponse.Ok(result.Message, result.Data));

            return BadRequest(ApiResponse.Fail(result.Message ?? "Customer creation failed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Embedly customer for Email={Email}", request.EmailAddress);
            return StatusCode(500, ApiResponse.Fail("An error occurred while creating the customer"));
        }
    }

    /// <summary>
    /// Get an Embedly customer by their customer ID
    /// </summary>
    /// <param name="customerId">The Embedly customer ID (UUID)</param>
    [HttpGet("customers/{customerId}")]
    public async Task<IActionResult> GetCustomerById(string customerId)
    {
        try
        {
            _logger.LogInformation("Fetching Embedly customer CustomerId={CustomerId}", customerId);

            var result = await _embedlyWalletService.GetCustomerByIdAsync(customerId);

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to retrieve customer from Embedly"));

            if (result.Data != null)
                return Ok(ApiResponse.Ok(result.Message, result.Data));

            return NotFound(ApiResponse.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Embedly customer CustomerId={CustomerId}", customerId);
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving the customer"));
        }
    }

    /// <summary>
    /// Get all Embedly customers for the organisation
    /// </summary>
    [HttpGet("customers")]
    public async Task<IActionResult> GetAllCustomers()
    {
        try
        {
            _logger.LogInformation("Fetching all Embedly customers");

            var result = await _embedlyWalletService.GetAllCustomersAsync();

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to retrieve customers from Embedly"));

            if (result.Data != null)
                return Ok(ApiResponse.Ok(result.Message, new { customers = result.Data, pagination = result.Pagination }));

            return BadRequest(ApiResponse.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all Embedly customers");
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving customers"));
        }
    }

    /// <summary>
    /// Create a new Embedly customer (v2 — full KYC)
    /// </summary>
    [HttpPost("customers/v2")]
    public async Task<IActionResult> CreateCustomerV2([FromBody] EmbedlyCreateCustomerV2RequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogInformation("Creating Embedly v2 customer for Email={Email}", request.EmailAddress);

            var result = await _embedlyWalletService.CreateCustomerV2Async(request);

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to create customer on Embedly"));

            if (result.Data != null)
                return Ok(ApiResponse.Ok(result.Message, result.Data));

            return BadRequest(ApiResponse.Fail(result.Message ?? "Customer creation failed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Embedly v2 customer for Email={Email}", request.EmailAddress);
            return StatusCode(500, ApiResponse.Fail("An error occurred while creating the customer"));
        }
    }

    /// <summary>
    /// Update an Embedly customer (v2 — full KYC)
    /// </summary>
    /// <param name="customerId">The Embedly customer ID (UUID)</param>
    [HttpPut("customers/v2/{customerId}")]
    public async Task<IActionResult> UpdateCustomerV2(string customerId, [FromBody] EmbedlyUpdateCustomerV2RequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogInformation("Updating Embedly v2 customer CustomerId={CustomerId}", customerId);

            var result = await _embedlyWalletService.UpdateCustomerV2Async(customerId, request);

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to update customer on Embedly"));

            if (result.Data != null)
                return Ok(ApiResponse.Ok(result.Message, result.Data));

            return BadRequest(ApiResponse.Fail(result.Message ?? "Customer update failed"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Embedly v2 customer CustomerId={CustomerId}", customerId);
            return StatusCode(500, ApiResponse.Fail("An error occurred while updating the customer"));
        }
    }

    /// <summary>
    /// Get KYC verification properties for an Embedly customer
    /// </summary>
    /// <param name="customerId">The Embedly customer ID (UUID)</param>
    [HttpGet("customers/{customerId}/kyc-status")]
    public async Task<IActionResult> GetKycStatus(string customerId)
    {
        try
        {
            _logger.LogInformation("Fetching KYC status for CustomerId={CustomerId}", customerId);

            var result = await _embedlyWalletService.GetKycStatusAsync(customerId);

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to retrieve KYC status from Embedly"));

            if (result.Data != null)
                return Ok(ApiResponse.Ok(result.Message, result.Data));

            return NotFound(ApiResponse.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching KYC status for CustomerId={CustomerId}", customerId);
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving KYC status"));
        }
    }

    /// <summary>
    /// Upgrade customer KYC tier via NIN verification
    /// </summary>
    /// <param name="customerId">The Embedly customer ID (UUID)</param>
    /// <param name="nin">The customer's NIN</param>
    [HttpPost("customers/{customerId}/kyc/nin/{nin}")]
    public async Task<IActionResult> NinKycUpgrade(string customerId, string nin, [FromBody] EmbedlyNinKycUpgradeRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogInformation("NIN KYC upgrade for CustomerId={CustomerId}", customerId);

            var result = await _embedlyWalletService.NinKycUpgradeAsync(customerId, nin, request);

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to perform NIN KYC upgrade on Embedly"));

            if (result.Success)
                return Ok(ApiResponse.Ok(result.Message, result.Data));

            return BadRequest(ApiResponse.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during NIN KYC upgrade for CustomerId={CustomerId}", customerId);
            return StatusCode(500, ApiResponse.Fail("An error occurred during NIN KYC upgrade"));
        }
    }

    /// <summary>
    /// Upgrade customer KYC tier via BVN verification
    /// </summary>
    [HttpPost("customers/kyc/bvn")]
    public async Task<IActionResult> BvnKycUpgrade([FromBody] EmbedlyBvnKycUpgradeRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogInformation("BVN KYC upgrade for CustomerId={CustomerId}", request.CustomerId);

            var result = await _embedlyWalletService.BvnKycUpgradeAsync(request);

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to perform BVN KYC upgrade on Embedly"));

            if (result.Success)
                return Ok(ApiResponse.Ok(result.Message, result.Data));

            return BadRequest(ApiResponse.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during BVN KYC upgrade for CustomerId={CustomerId}", request.CustomerId);
            return StatusCode(500, ApiResponse.Fail("An error occurred during BVN KYC upgrade"));
        }
    }

    /// <summary>
    /// Transfer funds between two Embedly wallets
    /// </summary>
    [HttpPost("transfer")]
    public async Task<IActionResult> WalletTransfer([FromBody] EmbedlyWalletTransferRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogInformation("Wallet transfer request From={From} To={To} Amount={Amount}",
                request.FromAccount, request.ToAccount, request.Amount);

            var result = await _embedlyWalletService.WalletTransferAsync(request);

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to process wallet transfer on Embedly"));

            if (result.Success)
                return Ok(ApiResponse.Ok(result.Message, result.Data));

            return BadRequest(ApiResponse.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing wallet transfer From={From} To={To}", request.FromAccount, request.ToAccount);
            return StatusCode(500, ApiResponse.Fail("An error occurred while processing the wallet transfer"));
        }
    }

    /// <summary>
    /// Simulate an inflow (fund a virtual account) — staging only
    /// </summary>
    [HttpPost("fund")]
    public async Task<IActionResult> FundAccount([FromBody] EmbedlyFundAccountRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogInformation("Fund account request AccountNumber={AccountNumber} Amount={Amount}",
                request.BeneficiaryAccountNumber, request.Amount);

            var result = await _embedlyWalletService.FundAccountAsync(request);

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to process fund account request on Embedly"));

            if (result.Success)
                return Ok(ApiResponse.Ok(result.Message, result.Data));

            return BadRequest(ApiResponse.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error funding account AccountNumber={AccountNumber}", request.BeneficiaryAccountNumber);
            return StatusCode(500, ApiResponse.Fail("An error occurred while funding the account"));
        }
    }

    // ─── Wallet Endpoints ─────────────────────────────────────────────────────

    /// <summary>
    /// Create a wallet for an Embedly customer
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateWallet([FromBody] EmbedlyCreateWalletRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            _logger.LogInformation("Creating Embedly wallet for CustomerId={CustomerId}", request.CustomerId);

            var result = await _embedlyWalletService.CreateWalletAsync(request);

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to create wallet on Embedly"));

            if (result.Status)
                return Ok(ApiResponse.Ok(result.Message, result));

            return BadRequest(ApiResponse.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Embedly wallet for CustomerId={CustomerId}", request.CustomerId);
            return StatusCode(500, ApiResponse.Fail("An error occurred while creating the wallet"));
        }
    }

    /// <summary>
    /// Get an Embedly wallet by its wallet ID
    /// </summary>
    /// <param name="walletId">The Embedly wallet ID (UUID)</param>
    [HttpGet("id/{walletId}")]
    public async Task<IActionResult> GetWalletById(string walletId)
    {
        try
        {
            _logger.LogInformation("Fetching Embedly wallet WalletId={WalletId}", walletId);

            var result = await _embedlyWalletService.GetWalletByIdAsync(walletId);

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to retrieve wallet from Embedly"));

            if (result.Data != null)
                return Ok(ApiResponse.Ok(result.Message, result.Data));

            return NotFound(ApiResponse.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Embedly wallet WalletId={WalletId}", walletId);
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving the wallet"));
        }
    }

    /// <summary>
    /// Get an Embedly wallet by its virtual account number
    /// </summary>
    /// <param name="accountNumber">The virtual account number</param>
    [HttpGet("account/{accountNumber}")]
    public async Task<IActionResult> GetWalletByAccountNumber(string accountNumber)
    {
        try
        {
            _logger.LogInformation("Fetching Embedly wallet AccountNumber={AccountNumber}", accountNumber);

            var result = await _embedlyWalletService.GetWalletByAccountNumberAsync(accountNumber);

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to retrieve wallet from Embedly"));

            if (result.Data != null)
                return Ok(ApiResponse.Ok(result.Message, result.Data));

            return NotFound(ApiResponse.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Embedly wallet AccountNumber={AccountNumber}", accountNumber);
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving the wallet"));
        }
    }

    /// <summary>
    /// Get all wallets belonging to an Embedly customer
    /// </summary>
    /// <param name="customerId">The Embedly customer ID (UUID)</param>
    [HttpGet("customers/{customerId}/wallets")]
    public async Task<IActionResult> GetWalletsByCustomerId(string customerId)
    {
        try
        {
            _logger.LogInformation("Fetching Embedly wallets for CustomerId={CustomerId}", customerId);

            var result = await _embedlyWalletService.GetWalletsByCustomerIdAsync(customerId);

            if (result == null)
                return BadRequest(ApiResponse.Fail("Failed to retrieve wallets from Embedly"));

            if (result.Data != null)
                return Ok(ApiResponse.Ok(result.Message, result.Data));

            return NotFound(ApiResponse.Fail(result.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching Embedly wallets for CustomerId={CustomerId}", customerId);
            return StatusCode(500, ApiResponse.Fail("An error occurred while retrieving wallets"));
        }
    }
}
