using Trivare.Application.DTOs.Files;

namespace Trivare.Application.DTOs.Files;

/// <summary>
/// Response DTO for successful file uploads
/// Extends FileDto with additional URLs and storage path
/// </summary>
public record FileUploadResponse : FileDto
{
    /// <summary>
    /// Storage path of the uploaded file
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// URL for previewing the file (if applicable)
    /// </summary>
    public required string PreviewUrl { get; init; }
}