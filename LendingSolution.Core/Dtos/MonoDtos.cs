using System.Text.Json.Serialization;

namespace LendingSolution.Core.Dtos;

// ─── Customer DTOs ────────────────────────────────────────────────────────────

public class MonoCustomerIdentityDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "BVN";

    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;
}

public class MonoCreateCustomerRequestDto
{
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = "individual";

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("identity")]
    public MonoCustomerIdentityDto Identity { get; set; } = new();
}

public class MonoCreateCustomerResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public MonoCustomerDataDto? Data { get; set; }
}

public class MonoCustomerConflictResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public MonoCustomerConflictDataDto? Data { get; set; }
}

public class MonoCustomerConflictDataDto
{
    [JsonPropertyName("existing_customer")]
    public MonoExistingCustomerDto? ExistingCustomer { get; set; }
}

public class MonoExistingCustomerDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

public class MonoCustomerDataDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

public class MonoGetCustomerResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public MonoCustomerDataDto? Data { get; set; }
}

public class MonoCustomerListMetaDto
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("pages")]
    public int Pages { get; set; }

    [JsonPropertyName("previous")]
    public string? Previous { get; set; }

    [JsonPropertyName("next")]
    public string? Next { get; set; }
}

public class MonoGetAllCustomersResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public List<MonoCustomerDataDto> Data { get; set; } = [];

    [JsonPropertyName("meta")]
    public MonoCustomerListMetaDto? Meta { get; set; }
}

public class MonoLinkedAccountDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("account_number")]
    public string AccountNumber { get; set; } = string.Empty;

    [JsonPropertyName("balance")]
    public long Balance { get; set; }

    [JsonPropertyName("bvn")]
    public string Bvn { get; set; } = string.Empty;

    [JsonPropertyName("institution")]
    public MonoLinkedAccountInstitutionDto? Institution { get; set; }

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;
}

public class MonoLinkedAccountInstitutionDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("bank_code")]
    public string BankCode { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}

public class MonoGetLinkedAccountsResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public List<MonoLinkedAccountDto> Data { get; set; } = [];
}

// ─── Request DTOs ─────────────────────────────────────────────────────────────
public class MonoMandateCustomerDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
}

public class MonoGenerateMandateRequestDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "recurring-debit";

    [JsonPropertyName("method")]
    public string Method { get; set; } = "mandate";

    [JsonPropertyName("mandate_type")]
    public string MandateType { get; set; } = "emandate";

    [JsonPropertyName("debit_type")]
    public string DebitType { get; set; } = "variable";

    [JsonPropertyName("customer")]
    public MonoMandateCustomerDto Customer { get; set; } = new();

    /// <summary>
    /// Amount in kobo (i.e. multiply Naira by 100).
    /// </summary>
    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("reference")]
    public string Reference { get; set; } = string.Empty;

    [JsonPropertyName("account_number")]
    public string AccountNumber { get; set; } = string.Empty;

    [JsonPropertyName("bank_code")]
    public string BankCode { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("start_date")]
    public string StartDate { get; set; } = string.Empty;

    [JsonPropertyName("end_date")]
    public string EndDate { get; set; } = string.Empty;

    [JsonPropertyName("redirect_url")]
    public string RedirectUrl { get; set; } = string.Empty;

    [JsonPropertyName("meta")]
    public object Meta { get; set; } = new { source = "devpay" };
}

// Response DTOs
public class MonoGenerateMandateResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("data")]
    public MonoMandateDataDto? Data { get; set; }
}

public class MonoMandateDataDto
{
    [JsonPropertyName("meta")]
    public object Meta { get; set; } = new { };
    
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("mandate_type")]
    public string MandateType { get; set; } = string.Empty;
    
    [JsonPropertyName("debit_type")]
    public string DebitType { get; set; } = string.Empty;
    
    [JsonPropertyName("ready_to_debit")]
    public bool ReadyToDebit { get; set; }
    
    [JsonPropertyName("nibss_code")]
    public string NibssCode { get; set; } = string.Empty;
    
    [JsonPropertyName("approved")]
    public bool Approved { get; set; }
    
    [JsonPropertyName("reference")]
    public string Reference { get; set; } = string.Empty;
    
    [JsonPropertyName("account_name")]
    public string AccountName { get; set; } = string.Empty;
    
    [JsonPropertyName("account_number")]
    public string AccountNumber { get; set; } = string.Empty;
    
    [JsonPropertyName("bank")]
    public string Bank { get; set; } = string.Empty;
    
    [JsonPropertyName("bank_code")]
    public string BankCode { get; set; } = string.Empty;
    
    [JsonPropertyName("customer")]
    public string Customer { get; set; } = string.Empty;
    
    [JsonPropertyName("fee_bearer")]
    public string FeeBearer { get; set; } = string.Empty;
    
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
    
    [JsonPropertyName("live_mode")]
    public bool LiveMode { get; set; }
    
    [JsonPropertyName("start_date")]
    public DateTime StartDate { get; set; }
    
    [JsonPropertyName("end_date")]
    public DateTime EndDate { get; set; }
    
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }
    
    [JsonPropertyName("initial_debit_date")]
    public DateTime InitialDebitDate { get; set; }
    
    [JsonPropertyName("transfer_destinations")]
    public List<MonoTransferDestinationDto> TransferDestinations { get; set; } = new();
    
    [JsonPropertyName("amount")]
    public int Amount { get; set; }
    
    [JsonPropertyName("initial_debit_amount")]
    public int InitialDebitAmount { get; set; }
}

public class MonoTransferDestinationDto
{
    [JsonPropertyName("bank_name")]
    public string BankName { get; set; } = string.Empty;
    
    [JsonPropertyName("account_number")]
    public long AccountNumber { get; set; }
    
    [JsonPropertyName("icon")]
    public string Icon { get; set; } = string.Empty;
    
    [JsonPropertyName("primary_color")]
    public string PrimaryColor { get; set; } = string.Empty;
}

// Cancel Mandate Response DTO
public class MonoCancelMandateResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("response_code")]
    public string ResponseCode { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonPropertyName("documentation")]
    public string Documentation { get; set; } = string.Empty;
    
    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

// Pause Mandate Response DTO
public class MonoPauseMandateResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("response_code")]
    public string ResponseCode { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("timestamps")]
    public DateTime Timestamps { get; set; }
    
    [JsonPropertyName("documentation")]
    public string Documentation { get; set; } = string.Empty;
    
    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

// Reinstate Mandate Response DTO
public class MonoReinstateMandateResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("response_code")]
    public string ResponseCode { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonPropertyName("documentation")]
    public string Documentation { get; set; } = string.Empty;
    
    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

// Banks Response DTOs
public class MonoBanksResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public List<MonoBankDto> Data { get; set; } = [];
}

public class MonoBankDto
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    
    [JsonPropertyName("bank_code")]
    public string BankCode { get; set; } = string.Empty;
    
    [JsonPropertyName("nip_code")]
    public string NipCode { get; set; } = string.Empty;
    
    [JsonPropertyName("direct_debit")]
    public bool DirectDebit { get; set; }
}

// BVN Lookup DTOs
// ─── NIN Lookup DTOs ─────────────────────────────────────────────────────────

public class MonoNinLookupRequestDto
{
    [JsonPropertyName("nin")]
    public string Nin { get; set; } = string.Empty;
}

public class MonoNinLookupResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("data")]
    public MonoNinLookupDataDto? Data { get; set; }
}

public class MonoNinLookupDataDto
{
    [JsonPropertyName("nin")]
    public string Nin { get; set; } = string.Empty;

    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("middle_name")]
    public string MiddleName { get; set; } = string.Empty;

    [JsonPropertyName("date_of_birth")]
    public string DateOfBirth { get; set; } = string.Empty;

    [JsonPropertyName("gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonPropertyName("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("state_of_origin")]
    public string StateOfOrigin { get; set; } = string.Empty;

    [JsonPropertyName("lga_of_origin")]
    public string LgaOfOrigin { get; set; } = string.Empty;

    [JsonPropertyName("photo")]
    public string Photo { get; set; } = string.Empty;
}

// ─── BVN Lookup DTOs ─────────────────────────────────────────────────────────

public class MonoBvnLookupRequestDto
{
    [JsonPropertyName("bvn")]
    public string Bvn { get; set; } = string.Empty;
    
    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
}

public class MonoBvnLookupResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonPropertyName("data")]
    public MonoBvnLookupDataDto? Data { get; set; }
}

public class MonoBvnLookupDataDto
{
    [JsonPropertyName("session_id")]
    public string SessionId { get; set; } = string.Empty;
    
    [JsonPropertyName("bvn")]
    public string Bvn { get; set; } = string.Empty;
    
    [JsonPropertyName("methods")]
    public List<MonoBvnMethodDto> Methods { get; set; } = new();
}

public class MonoBvnMethodDto
{
    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;
    
    [JsonPropertyName("hint")]
    public string Hint { get; set; } = string.Empty;
}

// BVN Verify Request/Response DTOs
public class MonoBvnVerifyRequestDto
{
    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;
    
    [JsonPropertyName("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;
}

public class MonoBvnVerifyResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

// BVN Details Request/Response DTOs
public class MonoBvnDetailsRequestDto
{
    [JsonPropertyName("otp")]
    public string Otp { get; set; } = string.Empty;
}

public class MonoBvnDetailsResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonPropertyName("data")]
    public MonoBvnDetailsDataDto? Data { get; set; }
}

public class MonoBvnDetailsDataDto
{
    [JsonPropertyName("first_name")]
    public string FirstName { get; set; } = string.Empty;
    
    [JsonPropertyName("last_name")]
    public string LastName { get; set; } = string.Empty;
    
    [JsonPropertyName("middle_name")]
    public string? MiddleName { get; set; }
    
    [JsonPropertyName("dob")]
    public string DateOfBirth { get; set; } = string.Empty;
    
    [JsonPropertyName("phone_number")]
    public string PhoneNumber { get; set; } = string.Empty;
    
    [JsonPropertyName("phone_number_2")]
    public string? PhoneNumber2 { get; set; }
    
    [JsonPropertyName("email")]
    public string Email { get; set; } = string.Empty;
    
    [JsonPropertyName("gender")]
    public string Gender { get; set; } = string.Empty;
    
    [JsonPropertyName("state_of_origin")]
    public string StateOfOrigin { get; set; } = string.Empty;
    
    [JsonPropertyName("bvn")]
    public string Bvn { get; set; } = string.Empty;
    
    [JsonPropertyName("nin")]
    public string Nin { get; set; } = string.Empty;
    
    [JsonPropertyName("registration_date")]
    public string RegistrationDate { get; set; } = string.Empty;
    
    [JsonPropertyName("lga_of_origin")]
    public string LgaOfOrigin { get; set; } = string.Empty;
    
    [JsonPropertyName("lga_of_Residence")]
    public string LgaOfResidence { get; set; } = string.Empty;
    
    [JsonPropertyName("marital_status")]
    public string MaritalStatus { get; set; } = string.Empty;
    
    [JsonPropertyName("watch_listed")]
    public bool WatchListed { get; set; }
    
    [JsonPropertyName("photoId")]
    public string? PhotoId { get; set; }
}

// Combined BVN Validation Result for easier service usage
public class MonoBvnValidationResultDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Bvn { get; set; } = string.Empty;
    public List<MonoBvnMethodDto> AvailableMethods { get; set; } = new();
    public MonoBvnDetailsDataDto? BvnDetails { get; set; }
    public bool IsValidated { get; set; }
    public string ValidationMessage { get; set; } = string.Empty;
}

// Helper DTO for complete validation endpoint
public class MonoBvnCompleteValidationRequestDto
{
    public string Bvn { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
    public string Scope { get; set; } = "identity";
}

// Credit History DTOs
public class MonoCreditHistoryRequestDto
{
    [JsonPropertyName("bvn")]
    public string Bvn { get; set; } = string.Empty;
}

public class MonoCreditHistoryResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonPropertyName("data")]
    public MonoCreditHistoryDataDto? Data { get; set; }
}

public class MonoCreditHistoryDataDto
{
    [JsonPropertyName("providers")]
    public List<string> Providers { get; set; } = new();
    
    [JsonPropertyName("profile")]
    public MonoCreditProfileDto Profile { get; set; } = new();
    
    [JsonPropertyName("credit_history")]
    public List<MonoCreditInstitutionDto> CreditHistory { get; set; } = new();
}

public class MonoCreditProfileDto
{
    [JsonPropertyName("full_name")]
    public string FullName { get; set; } = string.Empty;
    
    [JsonPropertyName("dob")]
    public string DateOfBirth { get; set; } = string.Empty;
    
    [JsonPropertyName("address_history")]
    public List<MonoAddressHistoryDto> AddressHistory { get; set; } = new();
    
    [JsonPropertyName("email_addresses")]
    public List<string> EmailAddresses { get; set; } = new();
    
    [JsonPropertyName("phone_numbers")]
    public List<string> PhoneNumbers { get; set; } = new();
    
    [JsonPropertyName("gender")]
    public string Gender { get; set; } = string.Empty;
    
    [JsonPropertyName("identifications")]
    public List<MonoIdentificationDto> Identifications { get; set; } = new();
}

public class MonoAddressHistoryDto
{
    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("date_reported")]
    public string DateReported { get; set; } = string.Empty;
}

public class MonoIdentificationDto
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    
    [JsonPropertyName("no")]
    public string Number { get; set; } = string.Empty;
}

public class MonoCreditInstitutionDto
{
    [JsonPropertyName("institution")]
    public string Institution { get; set; } = string.Empty;
    
    [JsonPropertyName("history")]
    public List<MonoCreditAccountDto> History { get; set; } = new();
}

public class MonoCreditAccountDto
{
    [JsonPropertyName("date_opened")]
    public string DateOpened { get; set; } = string.Empty;
    
    [JsonPropertyName("opening_balance")]
    public decimal OpeningBalance { get; set; }
    
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;
    
    [JsonPropertyName("performance_status")]
    public string PerformanceStatus { get; set; } = string.Empty;
    
    [JsonPropertyName("tenor")]
    public int Tenor { get; set; }
    
    [JsonPropertyName("closed_date")]
    public string? ClosedDate { get; set; }
    
    [JsonPropertyName("loan_status")]
    public string LoanStatus { get; set; } = string.Empty;
    
    [JsonPropertyName("repayment_frequency")]
    public string RepaymentFrequency { get; set; } = string.Empty;
    
    [JsonPropertyName("repayment_amount")]
    public decimal RepaymentAmount { get; set; }
    
    [JsonPropertyName("repayment_schedule")]
    public List<MonoRepaymentScheduleDto> RepaymentSchedule { get; set; } = new();
}

public class MonoRepaymentScheduleDto
{
    [JsonPropertyName("date")]
    public string Date { get; set; } = string.Empty;
    
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

// Credit Analysis Result (not stored, computed on-demand)
public class MonoCreditAnalysisResultDto
{
    public string Bvn { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public decimal CreditScore { get; set; }
    public decimal MaxLoanAmount { get; set; }
    public string RiskLevel { get; set; } = string.Empty; // Low, Medium, High
    public int ActiveLoansCount { get; set; }
    public decimal TotalOutstandingDebt { get; set; }
    public string OverallPerformanceStatus { get; set; } = string.Empty;
    public List<string> RiskFactors { get; set; } = new();
    public List<string> PositiveFactors { get; set; } = new();
    public string RecommendedAction { get; set; } = string.Empty; // Approve, Review, Decline
    public DateTime AnalyzedAt { get; set; } = DateTime.UtcNow;
}

// Session-based BVN DTOs
public class MonoBvnSessionRequestDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

public class MonoBvnSessionDetailsRequestDto
{
    public string SessionId { get; set; } = string.Empty;
    public string Otp { get; set; } = string.Empty;
}

// Error Response DTO
public class MonoErrorResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("errors")]
    public object? Errors { get; set; }
}

// Creditworthiness Check DTOs
public class MonoCreditworthinessRequestDto
{
    [JsonPropertyName("bvn")]
    public string Bvn { get; set; } = string.Empty;
    
    [JsonPropertyName("principal")]
    public decimal Principal { get; set; }
    
    [JsonPropertyName("interest_rate")]
    public decimal InterestRate { get; set; }
    
    [JsonPropertyName("term")]
    public int Term { get; set; }
    
    [JsonPropertyName("run_credit_check")]
    public bool RunCreditCheck { get; set; } = true;
    
    [JsonPropertyName("existing_loans")]
    public List<MonoExistingLoanDto>? ExistingLoans { get; set; }
}

public class MonoExistingLoanDto
{
    [JsonPropertyName("tenor")]
    public int Tenor { get; set; }
    
    [JsonPropertyName("date_opened")]
    public string DateOpened { get; set; } = string.Empty;
    
    [JsonPropertyName("closed_date")]
    public string? ClosedDate { get; set; }
    
    [JsonPropertyName("institution")]
    public string Institution { get; set; } = string.Empty;
    
    [JsonPropertyName("currency")]
    public string Currency { get; set; } = "NGN";
    
    [JsonPropertyName("repayment_amount")]
    public decimal RepaymentAmount { get; set; }
    
    [JsonPropertyName("opening_balance")]
    public decimal OpeningBalance { get; set; }
    
    [JsonPropertyName("repayment_schedule")]
    public List<Dictionary<string, string>>? RepaymentSchedule { get; set; }
}

public class MonoCreditworthinessResponseDto
{
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
    
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }
    
    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

// ─── Initiate Debit ────────────────────────────────────────────────────────

/// <summary>
/// Request to manually trigger a debit collection on an active Mono mandate.
/// POST /v3/payments/mandates/{mandateId}/debit
/// </summary>
public class MonoInitiateDebitRequestDto
{
    /// <summary>
    /// Amount in kobo (i.e. multiply Naira by 100).
    /// For variable mandates this can be any amount up to the mandate cap.
    /// Leave null to debit the mandate's default amount.
    /// </summary>
    [JsonPropertyName("amount")]
    public int? Amount { get; set; }

    /// <summary>
    /// Optional narration / description that appears on the bank statement.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}

/// <summary>Top-level response after initiating a debit on a Mono mandate.</summary>
public class MonoInitiateDebitResponseDto
{
    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("data")]
    public MonoInitiateDebitDataDto? Data { get; set; }
}

public class MonoInitiateDebitDataDto
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("amount")]
    public int Amount { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    [JsonPropertyName("mandate")]
    public string? Mandate { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime? CreatedAt { get; set; }
}