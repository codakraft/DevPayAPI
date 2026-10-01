namespace LendingSolution.Application.Exceptions;

public class AppException(string message, int statusCode = 400, string? errorCode = null) : Exception(message)
{
    public int StatusCode { get; } = statusCode;

    /// <summary>
    /// Optional machine-readable code surfaced to clients as <c>code</c> (see ErrorCodes).
    /// </summary>
    public string? ErrorCode { get; } = errorCode;
}
