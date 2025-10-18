namespace Trivare.Infrastructure.Settings;

/// <summary>
/// Settings for Cloudflare R2 storage
/// </summary>
public class CloudflareR2Settings
{
    /// <summary>
    /// Cloudflare Account ID
    /// </summary>
    public required string AccountId { get; set; }

    /// <summary>
    /// R2 Access Key ID
    /// </summary>
    public required string AccessKeyId { get; set; }

    /// <summary>
    /// R2 Secret Access Key
    /// </summary>
    public required string SecretAccessKey { get; set; }

    /// <summary>
    /// R2 Bucket Name
    /// </summary>
    public required string BucketName { get; set; }

    /// <summary>
    /// Public URL base for accessing files
    /// </summary>
    public required string PublicUrlBase { get; set; }
}