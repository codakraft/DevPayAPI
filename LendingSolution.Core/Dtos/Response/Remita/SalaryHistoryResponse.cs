namespace LendingSolution.Core.Dtos.Response.Remita;

public class SalaryHistoryResponse : ResponseBase
{
    public SalaryHistoryReviewData Data { get; set; } // Replace with real type if known
}

public class SalaryHistoryReviewData
{
    public string CompanyName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

}