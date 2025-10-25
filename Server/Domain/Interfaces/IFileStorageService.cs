namespace Trivare.Domain.Interfaces;

/// <summary>
/// Service for file storage operations
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to storage
    /// </summary>
    /// <param name="content">The file content stream</param>
    /// <param name="filePath">The storage path for the file</param>
    /// <param name="contentType">The MIME content type</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the upload operation</returns>
    Task UploadAsync(Stream content, string filePath, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a secure presigned download URL for a file with time-limited access
    /// </summary>
    /// <param name="filePath">The storage path of the file</param>
    /// <param name="expirationMinutes">Optional custom expiration time in minutes</param>
    /// <returns>The presigned download URL</returns>
    Task<string> GetPresignedDownloadUrlAsync(string filePath, int? expirationMinutes = null);

    /// <summary>
    /// Generates a secure presigned preview URL for a file with time-limited access
    /// </summary>
    /// <param name="filePath">The storage path of the file</param>
    /// <param name="expirationMinutes">Optional custom expiration time in minutes</param>
    /// <returns>The presigned preview URL</returns>
    Task<string> GetPresignedPreviewUrlAsync(string filePath, int? expirationMinutes = null);
}