namespace LendingSolution.Core.Settings
{
    public class JwtSettings
    {
        public string? Key { get; set; }
        public string? Issuer { get; set; }
        public string? Audience { get; set; }
        public int TokenExpiryInMinutes { get; set; }
        public int RefreshTokenExpiryInDays { get; set; } = 7; // Default to 7 days
    }
}