namespace LendingSolution.Application.Exceptions;

public class AppException(string message, int statusCode = 400, string? errorCode = null, object? details = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;

    /// <summary>
    /// Optional machine-readable code surfaced to clients as <c>code</c> (see ErrorCodes).
    /// </summary>
    public string? ErrorCode { get; } = errorCode;

    /// <summary>
    /// Optional structured payload surfaced to clients as <c>data</c> (e.g. remaining OTP attempts).
    /// </summary>
    public object? Details { get; } = details;
}
