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
}
