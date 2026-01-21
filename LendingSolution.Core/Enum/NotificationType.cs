namespace LendingSolution.Core.Enum;

/// <summary>
/// Types of notifications sent in the system
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Email verification OTP during borrower onboarding
    /// </summary>
    EmailVerificationOTP,
    
    /// <summary>
    /// BVN verification OTP during borrower onboarding
    /// </summary>
    BvnVerificationOTP,
    
    /// <summary>
    /// Loan offer letter with terms and conditions
    /// </summary>
    OfferLetter,
    
    /// <summary>
    /// Notification about successful loan disbursement
    /// </summary>
    DisbursementNotification,
    
    /// <summary>
    /// Summary of loan application details
    /// </summary>
    LoanApplicationSummary,
    
    /// <summary>
    /// Request for borrower to re-upload documents
    /// </summary>
    ImageReuploadRequest,
    
    /// <summary>
    /// General purpose notification
    /// </summary>
    GeneralNotification
}
