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
    /// Generates a download URL for a file
    /// </summary>
    /// <param name="filePath">The storage path of the file</param>
    /// <returns>The download URL</returns>
    string GetDownloadUrl(string filePath);

    /// <summary>
    /// Generates a preview URL for a file (if applicable)
    /// </summary>
    /// <param name="filePath">The storage path of the file</param>
    /// <returns>The preview URL</returns>
    string GetPreviewUrl(string filePath);
}