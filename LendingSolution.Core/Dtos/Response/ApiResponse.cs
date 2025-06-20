namespace LendingSolution.Core.Dtos.Response;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = String.Empty;
    public T? Data { get; set; } = default!;

}

public class ApiResponse : ApiResponse<object> { }