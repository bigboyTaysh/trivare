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

    /// <summary>
    /// Total number of files returned
    /// </summary>
    public required int Count { get; init; }

    /// <summary>
    /// Current number of files for the trip
    /// Used to show quota usage
    /// </summary>
    public required int CurrentFiles { get; init; }

    /// <summary>
    /// Maximum allowed files per trip (always 10)
    /// </summary>
    public required int MaxFiles { get; init; }
}
