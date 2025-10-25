using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trivare.Domain.Interfaces;
using Trivare.Infrastructure.Settings;

namespace Trivare.Infrastructure.Services;

/// <summary>
/// File storage service implementation using Cloudflare R2
/// </summary>
public class CloudflareR2FileStorageService : IFileStorageService
{
    private readonly CloudflareR2Settings _settings;
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<CloudflareR2FileStorageService> _logger;

    public CloudflareR2FileStorageService(
        IOptions<CloudflareR2Settings> settings,
        IAmazonS3 s3Client,
        ILogger<CloudflareR2FileStorageService> logger)
    {
        _settings = settings.Value;
        _s3Client = s3Client;
        _logger = logger;
    }

    /// <summary>
    /// Uploads a file to Cloudflare R2 storage with encryption and private access
    /// </summary>
    public async Task UploadAsync(Stream content, string filePath, string contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate file path to prevent path traversal attacks
            ValidateFilePath(filePath);

            var request = new PutObjectRequest
            {
                BucketName = _settings.BucketName,
                Key = filePath,
                InputStream = content,
                ContentType = contentType,
                // Private access - no public read
                CannedACL = S3CannedACL.Private,
                // Enable server-side encryption
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
                // Disable payload signing for Cloudflare R2 compatibility
                DisablePayloadSigning = true
            };

            var response = await _s3Client.PutObjectAsync(request, cancellationToken);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new InvalidOperationException($"Failed to upload file to R2. Status: {response.HttpStatusCode}");
            }

            _logger.LogInformation("Successfully uploaded encrypted file to R2: {FilePath}", filePath);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to R2: {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to upload file to R2: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generates a secure presigned download URL for a file with time-limited access
    /// </summary>
    public async Task<string> GetPresignedDownloadUrlAsync(string filePath, int? expirationMinutes = null)
    {
        try
        {
            ValidateFilePath(filePath);

            var expiration = expirationMinutes ?? _settings.PresignedUrlExpirationMinutes;
            var request = new GetPreSignedUrlRequest
            {
                BucketName = _settings.BucketName,
                Key = filePath,
                Expires = DateTime.UtcNow.AddMinutes(expiration),
                Verb = HttpVerb.GET,
                Protocol = Protocol.HTTPS
            };

            // GetPreSignedURL is synchronous despite the method name
            var url = _s3Client.GetPreSignedURL(request);
            _logger.LogDebug("Generated presigned download URL for {FilePath}, expires in {Minutes} minutes", filePath, expiration);
            return await Task.FromResult(url);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL for {FilePath}", filePath);
            throw new InvalidOperationException($"Failed to generate download URL: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Generates a secure presigned preview URL for a file with time-limited access
    /// </summary>
    public async Task<string> GetPresignedPreviewUrlAsync(string filePath, int? expirationMinutes = null)
    {
        // For now, same as download URL. Could add Content-Disposition: inline in the future
        return await GetPresignedDownloadUrlAsync(filePath, expirationMinutes);
    }

    /// <summary>
    /// Validates file path to prevent path traversal and injection attacks
    /// </summary>
    private void ValidateFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be empty", nameof(filePath));
        }

        // Prevent path traversal attacks
        if (filePath.Contains("..") || filePath.Contains("//") || filePath.StartsWith('/') || filePath.Contains('\\'))
        {
            _logger.LogWarning("Potential path traversal attack detected: {FilePath}", filePath);
            throw new ArgumentException("Invalid file path", nameof(filePath));
        }

        // Ensure path starts with expected prefix
        if (!filePath.StartsWith("trips/"))
        {
            _logger.LogWarning("File path does not start with expected prefix: {FilePath}", filePath);
            throw new ArgumentException("File path must start with 'trips/'", nameof(filePath));
        }
    }
}