namespace LendingSolution.Core.Settings;

/// <summary>
/// Firebase Storage configuration settings
/// </summary>
public class FirebaseSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
    public string AuthDomain { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string StorageBucket { get; set; } = string.Empty;
    public string MessagingSenderId { get; set; } = string.Empty;
    public string AppId { get; set; } = string.Empty;
    public string ServiceAccountKey { get; set; } = string.Empty; // Path to service account key file or JSON content
}
