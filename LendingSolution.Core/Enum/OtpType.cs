namespace LendingSolution.Core.Enum;

/// <summary>
/// Types of OTP verification used in the system
/// </summary>
public enum OtpType
{
    /// <summary>
    /// Email address verification during borrower onboarding
    /// </summary>
    EmailVerification = 1,
    
    /// <summary>
    /// Phone number verification
    /// </summary>
    PhoneVerification = 2,
    
    /// <summary>
    /// BVN (Bank Verification Number) verification during borrower onboarding
    /// </summary>
    BvnVerification = 3,
    
    /// <summary>
    /// Two-factor authentication for user login
    /// </summary>
    TwoFactorAuthentication = 4,
    
    /// <summary>
    /// Password reset verification
    /// </summary>
    PasswordReset = 5,
    
    /// <summary>
    /// Transaction authorization (high-value transactions)
    /// </summary>
    TransactionAuthorization = 6,
    
    /// <summary>
    /// Account recovery
    /// </summary>
    AccountRecovery = 7,
    
    /// <summary>
    /// Document signing verification
    /// </summary>
    DocumentSigning = 8,
    
    /// <summary>
    /// Admin login multi-factor authentication
    /// </summary>
    AdminLogin = 9,

    /// <summary>
    /// Borrower resuming an in-progress loan application
    /// </summary>
    ApplicationResume = 10
}
