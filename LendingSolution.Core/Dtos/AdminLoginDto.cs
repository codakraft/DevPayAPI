using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Dtos;

/// <summary>
/// Response from initial admin login containing session ID for MFA verification
/// </summary>
public class AdminLoginResponseDto
{
    /// <summary>
    /// Temporary session ID for MFA verification
    /// </summary>
    public required string SessionId { get; set; }
    
    /// <summary>
    /// When the OTP expires
    /// </summary>
    public DateTime ExpiresAt { get; set; }
    
    /// <summary>
    /// Masked email where OTP was sent (e.g., "ad***@example.com")
    /// </summary>
    public required string OtpSentTo { get; set; }
}

/// <summary>
/// Request to verify admin login OTP
/// </summary>
public class VerifyAdminLoginRequestDto
{
    /// <summary>
    /// Session ID from initial login
    /// </summary>
    [Required]
    public required string SessionId { get; set; }
    
    /// <summary>
    /// OTP code sent to admin's email
    /// </summary>
    [Required]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "OTP must be 6 digits")]
    public required string Otp { get; set; }
}

/// <summary>
/// Response from successful OTP verification with auth tokens
/// </summary>
public class VerifyAdminLoginResponseDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public required string TokenType { get; set; }
    public DateTime AccessTokenExpiry { get; set; }
    public DateTime RefreshTokenExpiry { get; set; }
    public required AdminUserDto User { get; set; }
}

/// <summary>
/// Admin user information returned after successful login
/// </summary>
public class AdminUserDto
{
    public required string Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? CompanyId { get; set; }
}
