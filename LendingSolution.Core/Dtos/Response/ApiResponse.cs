namespace LendingSolution.Core.Dtos.Response;

public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; } = null;

    /// <summary>
    /// Machine-readable error code (see <see cref="ErrorCodes"/>); omitted when not set.
    /// </summary>
    public string? Code { get; set; }

    public ApiResponse WithCode(string? code)
    {
        Code = code;
        return this;
    }
    public static ApiResponse Ok(string message) => new()
    {
        Success = true,
        Message = message
    };

    public static ApiResponse Ok(string message, object data) => new()
    {
        Success = true,
        Message = message,
        Data = data
    };

    public static ApiResponse Fail(string message) => new()
    {
        Success = false,
        Message = message
    };

    public static ApiResponse Fail(string message, object data) => new()
    {
        Success = false,
        Message = message,
        Data = data
    };
}

/// <summary>
/// Generic API response for Swagger documentation
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}
