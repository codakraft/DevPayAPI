namespace LendingSolution.Core.Settings;

public class EmbedlySettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string WaasCoreBaseUrl { get; set; } = string.Empty;
    public string WaasCoreV2BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string OrganizationId { get; set; } = string.Empty;
    public string CustomerTypeId { get; set; } = string.Empty;
    public string CurrencyId { get; set; } = string.Empty;
    public string CountryId { get; set; } = string.Empty;
    public bool PepDeclaration { get; set; } = false;
    public string EmploymentStatus { get; set; } = string.Empty;
    public int CustomerTierId { get; set; } = 0;
    public string SourceOfFunds { get; set; } = string.Empty;
}
