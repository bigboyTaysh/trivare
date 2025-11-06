namespace Trivare.Application.DTOs.Files;

/// <summary>
/// Response model for file listing operations
/// Includes file data and quota information
/// </summary>
public record FileListResponse
{
    /// <summary>
    /// Array of file metadata
    /// </summary>
    public required IEnumerable<FileDto> Data { get; init; }
}
