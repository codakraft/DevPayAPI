using System.Text.Json.Serialization;

namespace LendingSolution.Core.Dtos;

// ─── Customer Requests ────────────────────────────────────────────────────────

public class EmbedlyCreateCustomerRequestDto
{
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("middleName")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("dob")]
    public string Dob { get; set; } = string.Empty;

    [JsonPropertyName("emailAddress")]
    public string EmailAddress { get; set; } = string.Empty;

    [JsonPropertyName("mobileNumber")]
    public string MobileNumber { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("alias")]
    public string? Alias { get; set; }
}

// ─── Customer Responses ───────────────────────────────────────────────────────

public class EmbedlyCustomerDto
{
    [JsonPropertyName("id")]
    public string CustomerId { get; set; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("middleName")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("dob")]
    public string? DateOfBirth { get; set; }

    [JsonPropertyName("alias")]
    public string? Alias { get; set; }

    [JsonPropertyName("customerTypeId")]
    public string? CustomerTypeId { get; set; }

    [JsonPropertyName("customerTierId")]
    public int? CustomerTierId { get; set; }

    [JsonPropertyName("countryId")]
    public string? CountryId { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("emailAddress")]
    public string? Email { get; set; }

    [JsonPropertyName("mobileNumber")]
    public string? MobileNumber { get; set; }

    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("maritalStatus")]
    public string? MaritalStatus { get; set; }

    [JsonPropertyName("pepDeclaration")]
    public bool? PepDeclaration { get; set; }

    [JsonPropertyName("employmentStatus")]
    public string? EmploymentStatus { get; set; }

    [JsonPropertyName("occupation")]
    public string? Occupation { get; set; }

    [JsonPropertyName("sourceOfFunds")]
    public string? SourceOfFunds { get; set; }

    [JsonPropertyName("passportUrl")]
    public string? PassportUrl { get; set; }

    [JsonPropertyName("mothersMaidenName")]
    public string? MothersMaidenName { get; set; }

    [JsonPropertyName("kycTier")]
    public int? KycTier { get; set; }

    [JsonPropertyName("nextOfKin")]
    public EmbedlyNextOfKinDto? NextOfKin { get; set; }

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }

    [JsonPropertyName("updatedAt")]
    public string? UpdatedAt { get; set; }
}

public class EmbedlyCreateCustomerResponseDto
{
    [JsonPropertyName("success")]
    public bool Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public EmbedlyCustomerDto? Data { get; set; }
}

public class EmbedlyGetCustomerResponseDto
{
    [JsonPropertyName("status")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public EmbedlyCustomerDto? Data { get; set; }
}

public class EmbedlyGetAllCustomersResponseDto
{
    [JsonPropertyName("status")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public List<EmbedlyCustomerDto>? Data { get; set; }

    [JsonPropertyName("pagination")]
    public EmbedlyPaginationDto? Pagination { get; set; }
}

public class EmbedlyPaginationDto
{
    [JsonPropertyName("total")]
    public int Total { get; set; }

    [JsonPropertyName("page")]
    public int Page { get; set; }

    [JsonPropertyName("size")]
    public int Size { get; set; }
}

// ─── Wallet Requests ──────────────────────────────────────────────────────────

public class EmbedlyCreateWalletRequestDto
{
    [JsonPropertyName("customerId")]
    public string CustomerId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
}

// ─── Wallet Responses ─────────────────────────────────────────────────────────

public class EmbedlyVirtualAccountDto
{
    [JsonPropertyName("accountNumber")]
    public string? AccountNumber { get; set; }

    [JsonPropertyName("bankCode")]
    public string? BankCode { get; set; }

    [JsonPropertyName("bankName")]
    public string? BankName { get; set; }
}

public class EmbedlyWalletDto
{
    [JsonPropertyName("id")]
    public string WalletId { get; set; } = string.Empty;

    [JsonPropertyName("customerId")]
    public string? CustomerId { get; set; }

    [JsonPropertyName("walletGroupId")]
    public string? WalletGroupId { get; set; }

    [JsonPropertyName("currencyId")]
    public string? CurrencyId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("virtualAccount")]
    public EmbedlyVirtualAccountDto? VirtualAccount { get; set; }

    [JsonPropertyName("mobNum")]
    public string? MobileNumber { get; set; }

    [JsonPropertyName("availableBalance")]
    public decimal? AvailableBalance { get; set; }

    [JsonPropertyName("ledgerBalance")]
    public decimal? LedgerBalance { get; set; }

    [JsonPropertyName("isDefault")]
    public bool? IsDefault { get; set; }

    [JsonPropertyName("walletClassificationId")]
    public string? Classification { get; set; }

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }
}

public class EmbedlyCreateWalletResponseDto
{
    [JsonPropertyName("success")]
    public bool Status { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public EmbedlyWalletDto? Data { get; set; }
}

public class EmbedlyGetWalletResponseDto
{
    [JsonPropertyName("status")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public EmbedlyWalletDto? Data { get; set; }
}

public class EmbedlyGetWalletsByCustomerResponseDto
{
    [JsonPropertyName("status")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public List<EmbedlyWalletDto>? Data { get; set; }
}

// ─── Customer V2 Requests ─────────────────────────────────────────────────────

public class EmbedlyNextOfKinRequestDto
{
    [JsonPropertyName("surname")]
    public string Surname { get; set; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("otherNames")]
    public string? OtherNames { get; set; }

    [JsonPropertyName("relationship")]
    public string Relationship { get; set; } = string.Empty;

    [JsonPropertyName("mobileNumber")]
    public string MobileNumber { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;
}

public class EmbedlyCreateCustomerV2RequestDto
{
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("middleName")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("dob")]
    public string Dob { get; set; } = string.Empty;

    [JsonPropertyName("alias")]
    public string? Alias { get; set; }

    [JsonPropertyName("city")]
    public string City { get; set; } = string.Empty;

    [JsonPropertyName("address")]
    public string Address { get; set; } = string.Empty;

    [JsonPropertyName("mobileNumber")]
    public string MobileNumber { get; set; } = string.Empty;

    [JsonPropertyName("emailAddress")]
    public string EmailAddress { get; set; } = string.Empty;

    [JsonPropertyName("gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonPropertyName("maritalStatus")]
    public string MaritalStatus { get; set; } = string.Empty;

    [JsonPropertyName("occupation")]
    public string Occupation { get; set; } = string.Empty;

    [JsonPropertyName("passportUrl")]
    public string? PassportUrl { get; set; }

    [JsonPropertyName("mothersMaidenName")]
    public string? MothersMaidenName { get; set; }

    [JsonPropertyName("nextOfKin")]
    public EmbedlyNextOfKinRequestDto NextOfKin { get; set; } = new();
}

// ─── Customer V2 Responses ────────────────────────────────────────────────────

public class EmbedlyNextOfKinDto
{
    [JsonPropertyName("surname")]
    public string? Surname { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("otherNames")]
    public string? OtherNames { get; set; }

    [JsonPropertyName("relationship")]
    public string? Relationship { get; set; }

    [JsonPropertyName("mobileNumber")]
    public string? MobileNumber { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }
}

public class EmbedlyCustomerV2Dto
{
    [JsonPropertyName("id")]
    public string CustomerId { get; set; } = string.Empty;

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("middleName")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("dob")]
    public string? DateOfBirth { get; set; }

    [JsonPropertyName("emailAddress")]
    public string? Email { get; set; }

    [JsonPropertyName("mobileNumber")]
    public string? MobileNumber { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("alias")]
    public string? Alias { get; set; }

    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("maritalStatus")]
    public string? MaritalStatus { get; set; }

    [JsonPropertyName("occupation")]
    public string? Occupation { get; set; }

    [JsonPropertyName("passportUrl")]
    public string? PassportUrl { get; set; }

    [JsonPropertyName("mothersMaidenName")]
    public string? MothersMaidenName { get; set; }

    [JsonPropertyName("kycTier")]
    public int? KycTier { get; set; }

    [JsonPropertyName("nextOfKin")]
    public EmbedlyNextOfKinDto? NextOfKin { get; set; }

    [JsonPropertyName("createdAt")]
    public string? CreatedAt { get; set; }
}

public class EmbedlyCreateCustomerV2ResponseDto
{
    [JsonPropertyName("status")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public EmbedlyCustomerV2Dto? Data { get; set; }
}

// ─── Update Customer V2 ───────────────────────────────────────────────────────

public class EmbedlyUpdateCustomerV2RequestDto
{
    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("middleName")]
    public string? MiddleName { get; set; }

    [JsonPropertyName("dob")]
    public string? Dob { get; set; }

    [JsonPropertyName("city")]
    public string? City { get; set; }

    [JsonPropertyName("address")]
    public string? Address { get; set; }

    [JsonPropertyName("occupation")]
    public string? Occupation { get; set; }

    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("bvnverified")]
    public bool? BvnVerified { get; set; }

    [JsonPropertyName("ninVerified")]
    public bool? NinVerified { get; set; }

    [JsonPropertyName("bvn")]
    public string? Bvn { get; set; }

    [JsonPropertyName("nin")]
    public string? Nin { get; set; }

    [JsonPropertyName("maritalStatus")]
    public string? MaritalStatus { get; set; }

    [JsonPropertyName("passportUrl")]
    public string? PassportUrl { get; set; }

    [JsonPropertyName("nextOfKin")]
    public EmbedlyNextOfKinRequestDto? NextOfKin { get; set; }
}

public class EmbedlyUpdateCustomerV2ResponseDto
{
    [JsonPropertyName("status")]
    public int StatusCode { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public EmbedlyCustomerV2Dto? Data { get; set; }
}

// ─── Wallet Transfer ──────────────────────────────────────────────────────────

public class EmbedlyWalletTransferRequestDto
{
    [JsonPropertyName("fromAccount")]
    public string FromAccount { get; set; } = string.Empty;

    [JsonPropertyName("toAccount")]
    public string ToAccount { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("remarks")]
    public string Remarks { get; set; } = string.Empty;
}

public class EmbedlyWalletTransferResponseDto
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

// ─── Fund Account (Simulate Inflow) ──────────────────────────────────────────

public class EmbedlyFundAccountRequestDto
{
    [JsonPropertyName("beneficiaryAccountName")]
    public string BeneficiaryAccountName { get; set; } = string.Empty;

    [JsonPropertyName("beneficiaryAccountNumber")]
    public string BeneficiaryAccountNumber { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public string Amount { get; set; } = string.Empty;

    [JsonPropertyName("narration")]
    public string Narration { get; set; } = string.Empty;
}

public class EmbedlyFundAccountResponseDto
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

// ─── KYC Upgrade ─────────────────────────────────────────────────────────────

public class EmbedlyNinKycUpgradeRequestDto
{
    [JsonPropertyName("firstname")]
    public string FirstName { get; set; } = string.Empty;

    [JsonPropertyName("lastname")]
    public string LastName { get; set; } = string.Empty;

    [JsonPropertyName("dob")]
    public string Dob { get; set; } = string.Empty;
}

public class EmbedlyBvnKycUpgradeRequestDto
{
    [JsonPropertyName("customerId")]
    public string CustomerId { get; set; } = string.Empty;

    [JsonPropertyName("bvn")]
    public string Bvn { get; set; } = string.Empty;
}

public class EmbedlyKycUpgradeResponseDto
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public EmbedlyKycStatusDto? Data { get; set; }
}

// ─── KYC Status ───────────────────────────────────────────────────────────────

public class EmbedlyKycStatusDto
{
    [JsonPropertyName("customerId")]
    public string? CustomerId { get; set; }

    [JsonPropertyName("bvnVerified")]
    public bool? BvnVerified { get; set; }

    [JsonPropertyName("ninVerified")]
    public bool? NinVerified { get; set; }

    [JsonPropertyName("phoneVerified")]
    public bool? PhoneVerified { get; set; }

    [JsonPropertyName("emailVerified")]
    public bool? EmailVerified { get; set; }

    [JsonPropertyName("kycTier")]
    public int? KycTier { get; set; }

    [JsonPropertyName("bvn")]
    public string? Bvn { get; set; }

    [JsonPropertyName("nin")]
    public string? Nin { get; set; }
}

public class EmbedlyGetKycStatusResponseDto
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public EmbedlyKycStatusDto? Data { get; set; }
}
