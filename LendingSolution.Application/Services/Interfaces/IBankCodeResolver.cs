namespace LendingSolution.Application.Services.Interfaces;

/// <summary>
/// Translates the CBN bank codes stored on borrower records into the 6-digit NIBSS
/// institution codes that Providus NIP transfers require.
/// </summary>
/// <remarks>
/// Borrower bank codes are captured at onboarding in CBN format (Sterling = "232").
/// Providus NIPFundTransfer expects the NIBSS code for the same bank (Sterling = "000001").
/// The two are unrelated numbering systems, so the mapping must be looked up, never derived.
///
/// The stored code is deliberately left in CBN format because Mono's direct-debit mandate
/// creation reads the same field and expects that format. Translation happens here, at the
/// point of disbursement, so neither integration disturbs the other.
/// </remarks>
public interface IBankCodeResolver
{
    /// <summary>
    /// Resolve a CBN bank code to its NIBSS institution code.
    /// </summary>
    /// <param name="bankCode">The CBN bank code stored on the borrower application</param>
    /// <returns>
    /// The 6-digit NIBSS code, or null if the bank could not be resolved. Callers must
    /// treat null as a hard failure and must not fall back to the unresolved code.
    /// </returns>
    Task<string?> ResolveNipCodeAsync(string bankCode);
}
