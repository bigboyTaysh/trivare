using Microsoft.Extensions.Options;
using Trivare.Domain.Interfaces;
using Trivare.Infrastructure.Settings;

namespace Trivare.Infrastructure.Services;

/// <summary>
/// File storage service implementation using Cloudflare R2
/// NOTE: This is a stub implementation. AWS SDK S3 needs to be added for full functionality.
/// </summary>
public class CloudflareR2FileStorageService : IFileStorageService
{
    private readonly CloudflareR2Settings _settings;

    public CloudflareR2FileStorageService(IOptions<CloudflareR2Settings> settings)
    {
        _settings = settings.Value;
    }

    /// <summary>
    /// Uploads a file to Cloudflare R2 storage
    /// TODO: Implement with AWS SDK S3
    /// </summary>
    public async Task UploadAsync(Stream content, string filePath, string contentType, CancellationToken cancellationToken = default)
    {
        // Stub implementation - throw not implemented
        throw new NotImplementedException("AWS SDK S3 package needs to be added and configured for R2 storage");
    }

    /// <summary>
    /// Generates a download URL for a file
    /// </summary>
    public string GetDownloadUrl(string filePath)
    {
        return $"{_settings.PublicUrlBase}/{filePath}";
    }

    /// <summary>
    /// Generates a preview URL for a file
    /// </summary>
    public string GetPreviewUrl(string filePath)
    {
        // For simplicity, same as download URL
        return GetDownloadUrl(filePath);
    }
}