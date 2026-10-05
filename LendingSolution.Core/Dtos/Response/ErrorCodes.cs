namespace LendingSolution.Core.Dtos.Response;

/// <summary>
/// Machine-readable codes returned as <c>code</c> on error responses so clients
/// don't have to match on message text. Messages may change; codes won't.
/// </summary>
public static class ErrorCodes
{
    /// <summary>403: the user must change their password before anything else.</summary>
    public const string PasswordChangeRequired = "PASSWORD_CHANGE_REQUIRED";

    /// <summary>403: the account has been deactivated (login, refresh).</summary>
    public const string AccountDeactivated = "ACCOUNT_DEACTIVATED";

    /// <summary>403: the user lacks the permission or role the endpoint requires.</summary>
    public const string PermissionDenied = "PERMISSION_DENIED";

    /// <summary>400: the OTP was wrong; <c>data.remainingAttempts</c> says how many tries are left.</summary>
    public const string OtpInvalid = "OTP_INVALID";

    /// <summary>400: too many wrong attempts; a new code must be requested.</summary>
    public const string OtpLocked = "OTP_LOCKED";

    /// <summary>400: the OTP has expired, was already used, or was never issued; a new code must be requested.</summary>
    public const string OtpExpired = "OTP_EXPIRED";
}
