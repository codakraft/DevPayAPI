using LendingSolution.Application.Services.Interfaces;
using LendingSolution.Core.Dtos;
using LendingSolution.API.Models;
using LendingSolution.Application.Exceptions;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using LendingSolution.Core.Dtos.Response;

namespace LendingSolution.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class WalletController(IWalletService walletService, ILogger<WalletController> logger) : ControllerBase
{
    private readonly IWalletService _walletService = walletService;
    private readonly ILogger<WalletController> _logger = logger;

    /// <summary>
    /// Get wallet by ID
    /// </summary>
    [HttpGet("{walletId}")]
    public async Task<ActionResult<WalletDto>> GetWallet(Guid walletId)
    {
        try
        {
            var wallet = await _walletService.GetWalletByIdAsync(walletId);
            if (wallet == null)
            {
                return NotFound("Wallet not found");
            }

            // Check authorization - users can only see their company's wallet or SuperAdmin can see all
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userCompanyId = User.FindFirst("CompanyId")?.Value;

            if (userRole != "SuperAdmin")
            {
                // Parse both GUIDs for proper comparison (case-insensitive)
                if (string.IsNullOrEmpty(userCompanyId) || 
                    !Guid.TryParse(userCompanyId, out var userCompanyGuid) || 
                    wallet.CompanyId != userCompanyGuid)
                {
                    return StatusCode(403, "You can only access your company's wallet");
                }
            }

            return Ok(wallet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting wallet: {WalletId}", walletId);
            return StatusCode(500, "An error occurred while retrieving the wallet");
        }
    }

    /// <summary>
    /// Get wallet by company ID or current user's company wallet
    /// </summary>
    [HttpGet("company/{companyId?}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<WalletDto>> GetCompanyWallet(Guid? companyId = null)
    {
        try
        {
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userCompanyId = User.FindFirst("CompanyId")?.Value;
            
            Guid targetCompanyId;
            
            // If no companyId provided, use current user's company
            if (!companyId.HasValue)
            {
                if (string.IsNullOrEmpty(userCompanyId) || !Guid.TryParse(userCompanyId, out targetCompanyId))
                {
                    return StatusCode(403, "Company information not found for user");
                }
            }
            else
            {
                targetCompanyId = companyId.Value;
                
                // SuperAdmin can access any company wallet, Admin can only access their own
                if (userRole != "SuperAdmin")
                {
                    if (string.IsNullOrEmpty(userCompanyId) || !Guid.TryParse(userCompanyId, out var userCompanyGuid) || userCompanyGuid != targetCompanyId)
                    {
                        return StatusCode(403, "You can only access your company's wallet");
                    }
                }
            }

            var wallet = await _walletService.GetWalletByCompanyIdAsync(targetCompanyId);
            if (wallet == null)
            {
                // Automatically create a wallet for the company if it doesn't exist
                try
                {
                    _logger.LogInformation("Company wallet not found for {CompanyId}, creating new wallet", targetCompanyId);
                    wallet = await _walletService.CreateCompanyWalletAsync(targetCompanyId);
                    _logger.LogInformation("Successfully created wallet for company {CompanyId}", targetCompanyId);
                }
                catch (AppException ex)
                {
                    _logger.LogError(ex, "Failed to create wallet for company {CompanyId}: {Message}", targetCompanyId, ex.Message);
                    return BadRequest($"Failed to create company wallet: {ex.Message}");
                }
            }

            return Ok(ApiResponse.Ok("Wallet fetched successfully", wallet));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting company wallet: {CompanyId}", companyId);
            return StatusCode(500, "An error occurred while retrieving the company wallet");
        }
    }

    /// <summary>
    /// Get SuperAdmin wallet (SuperAdmin only)
    /// </summary>
    [HttpGet("superadmin/wallet")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<WalletDto>> GetSuperAdminWallet()
    {
        try
        {
            var wallet = await _walletService.GetSuperAdminWalletAsync();
            if (wallet == null)
            {
                return NotFound("SuperAdmin wallet not found");
            }

            return Ok(wallet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting SuperAdmin wallet");
            return StatusCode(500, "An error occurred while retrieving the SuperAdmin wallet");
        }
    }

    /// <summary>
    /// Get all wallets (SuperAdmin only)
    /// </summary>
    [HttpGet("superadmin")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<List<WalletDto>>> GetAllWallets()
    {
        try
        {
            var wallets = await _walletService.GetAllWalletsAsync();
            return Ok(wallets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all wallets");
            return StatusCode(500, "An error occurred while retrieving wallets");
        }
    }

    /// <summary>
    /// Get wallet transactions
    /// </summary>
    [HttpGet("{walletId}/transactions")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<WalletTransactionDto>>> GetWalletTransactions(
        Guid walletId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            // Check authorization
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userCompanyId = User.FindFirst("CompanyId")?.Value;

            var wallet = await _walletService.GetWalletByIdAsync(walletId);
            if (wallet == null)
            {
                return NotFound("Wallet not found");
            }

            if (userRole != "SuperAdmin")
            {
                if (wallet.IsSuperAdminWallet)
                {
                    return StatusCode(403, "You can only view your company's wallet transactions");
                }
                
                // Parse both GUIDs for proper comparison (case-insensitive)
                if (string.IsNullOrEmpty(userCompanyId) || 
                    !Guid.TryParse(userCompanyId, out var userCompanyGuid) || 
                    wallet.CompanyId != userCompanyGuid)
                {
                    return StatusCode(403, "You can only view your company's wallet transactions");
                }
            }

            var transactions = await _walletService.GetWalletTransactionsAsync(walletId, page, pageSize);
            return Ok(transactions);
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error getting wallet transactions");
            return StatusCode(ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error getting wallet transactions");
            return StatusCode(500, "Something went wrong");
        }
    }

    /// <summary>
    /// Get wallet report
    /// </summary>
    [HttpGet("{walletId}/report")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<WalletReportDto>> GetWalletReport(
        Guid walletId,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            // Check authorization
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userCompanyId = User.FindFirst("CompanyId")?.Value;

            var wallet = await _walletService.GetWalletByIdAsync(walletId);
            if (wallet == null)
            {
                return NotFound("Wallet not found");
            }

            if (userRole != "SuperAdmin")
            {
                if (wallet.IsSuperAdminWallet)
                {
                    return StatusCode(403, "You can only view your company's wallet report");
                }
                
                // Parse both GUIDs for proper comparison (case-insensitive)
                if (string.IsNullOrEmpty(userCompanyId) || 
                    !Guid.TryParse(userCompanyId, out var userCompanyGuid) || 
                    wallet.CompanyId != userCompanyGuid)
                {
                    return StatusCode(403, "You can only view your company's wallet report");
                }
            }

            var report = await _walletService.GetWalletReportAsync(walletId, fromDate, toDate);
            return Ok(report);
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error getting wallet report");
            return StatusCode(ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error getting wallet report");
            return StatusCode(500, "Something went wrong");
        }
    }

    /// <summary>
    /// Check if wallet has sufficient balance for loan disbursement
    /// </summary>
    [HttpGet("{walletId}/balance-check")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<bool>> CheckSufficientBalance(Guid walletId, [FromQuery] decimal amount)
    {
        try
        {
            // Check authorization
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userCompanyId = User.FindFirst("CompanyId")?.Value;

            var wallet = await _walletService.GetWalletByIdAsync(walletId);
            if (wallet == null)
            {
                return NotFound("Wallet not found");
            }

            if (userRole != "SuperAdmin")
            {
                if (wallet.IsSuperAdminWallet)
                {
                    return StatusCode(403, "You can only check your company's wallet balance");
                }
                
                // Parse both GUIDs for proper comparison (case-insensitive)
                if (string.IsNullOrEmpty(userCompanyId) || 
                    !Guid.TryParse(userCompanyId, out var userCompanyGuid) || 
                    wallet.CompanyId != userCompanyGuid)
                {
                    return StatusCode(403, "You can only check your company's wallet balance");
                }
            }

            var hasSufficientBalance = await _walletService.HasSufficientBalanceAsync(walletId, amount);
            return Ok(new { hasSufficientBalance, currentBalance = wallet.Balance, requiredAmount = amount });
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error checking wallet balance");
            return StatusCode(ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error checking wallet balance");
            return StatusCode(500, "Something went wrong");
        }
    }

    /// <summary>
    /// Get current user's company wallet
    /// </summary>
    [HttpGet("my-company")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<WalletDto>> GetMyCompanyWallet()
    {
        try
        {
            var userCompanyId = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(userCompanyId) || !Guid.TryParse(userCompanyId, out var companyId))
            {
                return StatusCode(403, "Company information not found for user");
            }

            var wallet = await _walletService.GetWalletByCompanyIdAsync(companyId);
            if (wallet == null)
            {
                // Automatically create a wallet for the company if it doesn't exist
                try
                {
                    _logger.LogInformation("Company wallet not found for {CompanyId}, creating new wallet", companyId);
                    wallet = await _walletService.CreateCompanyWalletAsync(companyId);
                    _logger.LogInformation("Successfully created wallet for company {CompanyId}", companyId);
                }
                catch (AppException ex)
                {
                    _logger.LogError(ex, "Failed to create wallet for company {CompanyId}: {Message}", companyId, ex.Message);
                    return BadRequest($"Failed to create company wallet: {ex.Message}");
                }
            }

            return Ok(ApiResponse.Ok("Wallet fetched successfully", wallet));
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error getting company wallet");
            return StatusCode(ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error getting company wallet");
            return StatusCode(500, "Something went wrong");
        }
    }

    /// <summary>
    /// Get current user's company wallet transactions
    /// </summary>
    [HttpGet("my-company/transactions")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<WalletTransactionDto>>> GetMyCompanyWalletTransactions(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var userCompanyId = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(userCompanyId) || !Guid.TryParse(userCompanyId, out var companyId))
            {
                return StatusCode(403, "Company information not found for user");
            }

            var wallet = await _walletService.GetWalletByCompanyIdAsync(companyId);
            if (wallet == null)
            {
                return NotFound("Company wallet not found");
            }

            var transactions = await _walletService.GetWalletTransactionsAsync(wallet.Id, page, pageSize);
            return Ok(transactions);
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error getting company wallet transactions");
            return StatusCode(ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error getting company wallet transactions");
            return StatusCode(500, "Something went wrong");
        }
    }

    /// <summary>
    /// Get current user's company wallet report
    /// </summary>
    [HttpGet("my-company/report")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<WalletReportDto>> GetMyCompanyWalletReport(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var userCompanyId = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(userCompanyId) || !Guid.TryParse(userCompanyId, out var companyId))
            {
                return StatusCode(403, "Company information not found for user");
            }

            var wallet = await _walletService.GetWalletByCompanyIdAsync(companyId);
            if (wallet == null)
            {
                return NotFound("Company wallet not found");
            }

            var report = await _walletService.GetWalletReportAsync(wallet.Id, fromDate, toDate);
            return Ok(report);
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error getting company wallet report");
            return StatusCode(ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error getting company wallet report");
            return StatusCode(500, "Something went wrong");
        }
    }

    /// <summary>
    /// Check current user's company wallet balance
    /// </summary>
    [HttpGet("my-company/balance-check")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<bool>> CheckMyCompanyWalletBalance([FromQuery] decimal amount)
    {
        try
        {
            var userCompanyId = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(userCompanyId) || !Guid.TryParse(userCompanyId, out var companyId))
            {
                return StatusCode(403, "Company information not found for user");
            }

            var wallet = await _walletService.GetWalletByCompanyIdAsync(companyId);
            if (wallet == null)
            {
                return NotFound("Company wallet not found");
            }

            var hasSufficientBalance = await _walletService.HasSufficientBalanceAsync(wallet.Id, amount);
            return Ok(new { hasSufficientBalance, currentBalance = wallet.Balance, requiredAmount = amount });
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error checking company wallet balance");
            return StatusCode(ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error checking company wallet balance");
            return StatusCode(500, "Something went wrong");
        }
    }

    /// <summary>
    /// Get wallet transactions by query
    /// </summary>
    [HttpPost("transactions/query")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<WalletTransactionDto>>> GetTransactionsByQuery([FromBody] WalletTransactionQueryDto query)
    {
        try
        {
            // Check authorization
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userCompanyId = User.FindFirst("CompanyId")?.Value;

            if (query.WalletId.HasValue && userRole != "SuperAdmin")
            {
                var wallet = await _walletService.GetWalletByIdAsync(query.WalletId.Value);
                if (wallet == null)
                {
                    return NotFound("Wallet not found");
                }

                if (wallet.IsSuperAdminWallet)
                {
                    return StatusCode(403, "You can only query your company's wallet transactions");
                }
                
                // Parse both GUIDs for proper comparison (case-insensitive)
                if (string.IsNullOrEmpty(userCompanyId) || 
                    !Guid.TryParse(userCompanyId, out var userCompanyGuid) || 
                    wallet.CompanyId != userCompanyGuid)
                {
                    return StatusCode(403, "You can only query your company's wallet transactions");
                }
            }

            var transactions = await _walletService.GetTransactionsByQueryAsync(query);
            return Ok(transactions);
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error querying wallet transactions");
            return StatusCode(ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error querying wallet transactions");
            return StatusCode(500, "Something went wrong");
        }
    }

    /// <summary>
    /// Query current user's company wallet transactions
    /// </summary>
    [HttpPost("my-company/transactions/query")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<List<WalletTransactionDto>>> QueryMyCompanyWalletTransactions([FromBody] WalletTransactionQueryDto query)
    {
        try
        {
            var userCompanyId = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(userCompanyId) || !Guid.TryParse(userCompanyId, out var companyId))
            {
                return StatusCode(403, "Company information not found for user");
            }

            var wallet = await _walletService.GetWalletByCompanyIdAsync(companyId);
            if (wallet == null)
            {
                return NotFound("Company wallet not found");
            }

            // Override the WalletId in the query with the user's company wallet
            query.WalletId = wallet.Id;

            var transactions = await _walletService.GetTransactionsByQueryAsync(query);
            return Ok(transactions);
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error querying company wallet transactions");
            return StatusCode(ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error querying company wallet transactions");
            return StatusCode(500, "Something went wrong");
        }
    }

    /// <summary>
    /// Create wallet for company
    /// </summary>
    [HttpPost("company/{companyId}")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<WalletDto>> CreateCompanyWallet(Guid companyId)
    {
        try
        {
            // Check authorization - Admin can only create wallet for their company
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userCompanyId = User.FindFirst("CompanyId")?.Value;

            if (userRole != "SuperAdmin")
            {
                // Parse both GUIDs for proper comparison (case-insensitive)
                if (string.IsNullOrEmpty(userCompanyId) || 
                    !Guid.TryParse(userCompanyId, out var userCompanyGuid) || 
                    companyId != userCompanyGuid)
                {
                    return StatusCode(403, "You can only create a wallet for your company");
                }
            }

            var wallet = await _walletService.CreateCompanyWalletAsync(companyId);
            return CreatedAtAction(nameof(GetWallet), new { walletId = wallet.Id }, wallet);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating company wallet: {CompanyId}", companyId);
            return StatusCode(500, "An error occurred while creating the company wallet");
        }
    }

    /// <summary>
    /// Create SuperAdmin wallet (SuperAdmin only)
    /// </summary>
    [HttpPost("superadmin/wallet")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult<WalletDto>> CreateSuperAdminWallet()
    {
        try
        {
            var wallet = await _walletService.CreateSuperAdminWalletAsync();
            return CreatedAtAction(nameof(GetSuperAdminWallet), wallet);
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error creating SuperAdmin wallet");
            return StatusCode(ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error creating SuperAdmin wallet");
            return StatusCode(500, "Something went wrong");
        }
    }

    /// <summary>
    /// Initiate wallet funding via Paystack
    /// </summary>
    [HttpPost("fund")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult<PaystackInitializationDto>> FundWallet([FromBody] FundWalletDto fundWalletDto)
    {
        try
        {
            // Check authorization
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userCompanyId = User.FindFirst("CompanyId")?.Value;

            // Get wallet to check ownership
            var wallet = await _walletService.GetWalletByIdAsync(fundWalletDto.WalletId);
            if (wallet == null)
            {
                return NotFound("Wallet not found");
            }

            if (userRole != "SuperAdmin")
            {
                if (wallet.IsSuperAdminWallet)
                {
                    return StatusCode(403, "You can only fund your company's wallet");
                }
                
                // Parse both GUIDs for proper comparison (case-insensitive)
                if (string.IsNullOrEmpty(userCompanyId) || 
                    !Guid.TryParse(userCompanyId, out var userCompanyGuid) || 
                    wallet.CompanyId != userCompanyGuid)
                {
                    return StatusCode(403, "You can only fund your company's wallet");
                }
            }

            var result = await _walletService.InitiateWalletFundingAsync(fundWalletDto);
            return Ok(result);
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error initiating wallet funding");
            return StatusCode(ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error initiating wallet funding");
            return StatusCode(500, "Something went wrong");
        }
    }

    /// <summary>
    /// Complete wallet funding after Paystack payment verification
    /// </summary>
    [HttpPost("fund/complete")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult> CompleteFunding([FromBody] CompleteFundingDto completeFundingDto)
    {
        try
        {
            var result = await _walletService.CompleteWalletFundingAsync(completeFundingDto.PaystackReference);
            if (result)
            {
                return Ok(new { message = "Wallet funding completed successfully" });
            }

            return BadRequest("Failed to complete wallet funding");
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error completing wallet funding");
            return StatusCode(ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error completing wallet funding");
            return StatusCode(500, "Something went wrong");
        }
    }

    /// <summary>
    /// Debit a wallet
    /// </summary>
    [HttpPatch("{walletId}/debit")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<ActionResult> DebitWallet(Guid walletId, [FromBody] DebitWalletDto debitWalletDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var userCompanyId = User.FindFirst("CompanyId")?.Value;

            // Override the WalletId from the route parameter
            debitWalletDto.WalletId = walletId;

            // Get wallet to check ownership
            var wallet = await _walletService.GetWalletByIdAsync(debitWalletDto.WalletId);
            if (wallet == null)
            {
                return NotFound("Wallet not found");
            }

            // Authorization check
            if (userRole != "SuperAdmin")
            {
                if (wallet.IsSuperAdminWallet)
                {
                    return StatusCode(403, "You can only debit your company's wallet");
                }
                
                // Parse both GUIDs for proper comparison (case-insensitive)
                if (string.IsNullOrEmpty(userCompanyId) || 
                    !Guid.TryParse(userCompanyId, out var userCompanyGuid) || 
                    wallet.CompanyId != userCompanyGuid)
                {
                    return StatusCode(403, "You can only debit your company's wallet");
                }
            }

            var result = await _walletService.DebitWalletAsync(debitWalletDto, userId);
            if (result)
            {
                return Ok(new { message = "Wallet debited successfully" });
            }

            return BadRequest("Failed to debit wallet. Check balance and try again.");
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error debiting wallet");
            return StatusCode(ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error debiting wallet");
            return StatusCode(500, "Something went wrong");
        }
    }

    /// <summary>
    /// Transfer funds between wallets (SuperAdmin only)
    /// </summary>
    [HttpPost("superadmin/transfer")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<ActionResult> TransferFunds([FromBody] TransferFundsDto transferDto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value!;

            var result = await _walletService.TransferFundsAsync(
                transferDto.FromWalletId,
                transferDto.ToWalletId,
                transferDto.Amount,
                transferDto.Description,
                userId);

            if (result)
            {
                return Ok(new { message = "Funds transferred successfully" });
            }

            return BadRequest("Failed to transfer funds");
        }
        catch (AppException ex)
        {
            _logger.LogError(ex, "Error transferring funds");
            return StatusCode(ex.StatusCode, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error transferring funds");
            return StatusCode(500, "Something went wrong");
        }
    }
}
