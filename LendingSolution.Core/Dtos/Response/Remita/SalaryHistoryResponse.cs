namespace LendingSolution.Core.Dtos.Response.Remita;

public class SalaryHistoryResponse
{
    public string? Status { get; set; }
    public string? Message { get; set; }
    public required Data Data { get; set; } // Replace with real type if known
}

public class Data
{
    public string CompanyName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

}