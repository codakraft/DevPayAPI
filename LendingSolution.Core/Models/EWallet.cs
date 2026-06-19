using System.ComponentModel.DataAnnotations;

namespace LendingSolution.Core.Models;

public class EWallet : Base
{
    // ─── Customer fields (populated on CreateCustomer) ────────────────────────

    [MaxLength(100)]
    public string EmbedlyCustomerId { get; set; } = string.Empty;

    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? MiddleName { get; set; }

    [MaxLength(256)]
    public string? EmailAddress { get; set; }

    [MaxLength(20)]
    public string? MobileNumber { get; set; }

    [MaxLength(20)]
    public string? Dob { get; set; }

    [MaxLength(200)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? Alias { get; set; }

    public int? KycTier { get; set; }

    [MaxLength(20)]
    public string? Gender { get; set; }

    [MaxLength(50)]
    public string? MaritalStatus { get; set; }

    [MaxLength(100)]
    public string? Occupation { get; set; }

    [MaxLength(500)]
    public string? PassportUrl { get; set; }

    [MaxLength(100)]
    public string? MothersMaidenName { get; set; }

    [MaxLength(20)]
    public string? Bvn { get; set; }

    public bool? BvnVerified { get; set; }

    [MaxLength(20)]
    public string? Nin { get; set; }

    public bool? NinVerified { get; set; }

    // ─── Next of kin (flat columns) ───────────────────────────────────────────

    [MaxLength(100)]
    public string? NextOfKinSurname { get; set; }

    [MaxLength(100)]
    public string? NextOfKinFirstName { get; set; }

    [MaxLength(100)]
    public string? NextOfKinOtherNames { get; set; }

    [MaxLength(100)]
    public string? NextOfKinRelationship { get; set; }

    [MaxLength(20)]
    public string? NextOfKinMobileNumber { get; set; }

    [MaxLength(200)]
    public string? NextOfKinAddress { get; set; }

    [MaxLength(4000)]
    public string? CustomerRawResponse { get; set; }

    // ─── Wallet fields (populated on CreateWallet) ────────────────────────────

    [MaxLength(100)]
    public string? EmbedlyWalletId { get; set; }

    [MaxLength(100)]
    public string? WalletGroupId { get; set; }

    [MaxLength(100)]
    public string? CurrencyId { get; set; }

    [MaxLength(20)]
    public string? AccountNumber { get; set; }

    [MaxLength(20)]
    public string? BankCode { get; set; }

    [MaxLength(100)]
    public string? BankName { get; set; }

    [MaxLength(200)]
    public string? AccountName { get; set; }

    public decimal? AvailableBalance { get; set; }

    public decimal? LedgerBalance { get; set; }

    public bool? IsDefault { get; set; }

    [MaxLength(50)]
    public string? Classification { get; set; }

    [MaxLength(1000)]
    public string? WalletRawResponse { get; set; }
}
