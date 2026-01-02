namespace LendingSolution.Core.Dtos;

/// <summary>
/// Request DTO for getting salary history from Remita
/// </summary>
public class RemitaSalaryHistoryRequestDto
{
    public string AccountNumber { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string Bvn { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public Guid BorrowerApplicationId { get; set; }
    public string AuthorisationCode { get; set; } = string.Empty;
    public string AuthorisationChannel { get; set; } = "USSD";
}

/// <summary>
/// Request DTO for creating a mandate in Remita
/// </summary>
public class RemitaCreateMandateRequestDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string LoanAmount { get; set; } = string.Empty;
    public string CollectionAmount { get; set; } = string.Empty;
    public string DateOfDisbursement { get; set; } = string.Empty;
    public string DateOfCollection { get; set; } = string.Empty;
    public string TotalCollectionAmount { get; set; } = string.Empty;
    public string NumberOfRepayments { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string AuthorisationCode { get; set; } = string.Empty;
    public string AuthorisationChannel { get; set; } = "USSD";
}

/// <summary>
/// Request DTO for stopping a mandate in Remita
/// </summary>
public class RemitaStopMandateRequestDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string MandateReference { get; set; } = string.Empty;
    public string AuthorisationCode { get; set; } = string.Empty;
}

/// <summary>
/// Request DTO for getting mandate history from Remita
/// </summary>
public class RemitaMandateHistoryRequestDto
{
    public string CustomerId { get; set; } = string.Empty;
    public string MandateReference { get; set; } = string.Empty;
    public string AuthorisationCode { get; set; } = string.Empty;
}
