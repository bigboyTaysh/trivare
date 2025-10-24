namespace Trivare.Application.DTOs.Files;

/// <summary>
/// File metadata data transfer object
/// Derived from File entity
/// Note: Does not include file content, only metadata
/// </summary>
public record FileDto
{
    /// <summary>
    /// File identifier from File.Id
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Original file name from File.FileName
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// File size in bytes from File.FileSize
    /// </summary>
    public required long FileSize { get; init; }

    /// <summary>
    /// MIME type from File.FileType
    /// </summary>
    public required string FileType { get; init; }

    /// <summary>
    /// Parent trip identifier from File.TripId (null if associated with transport/accommodation/day)
    /// </summary>
    public Guid? TripId { get; init; }

    /// <summary>
    /// Associated transport identifier from File.TransportId
    /// </summary>
    public Guid? TransportId { get; init; }

    /// <summary>
    /// Associated accommodation identifier from File.AccommodationId
    /// </summary>
    public Guid? AccommodationId { get; init; }

    /// <summary>
    /// Associated day identifier from File.DayId
    /// </summary>
    public Guid? DayId { get; init; }

    /// <summary>
    /// File upload timestamp from File.CreatedAt
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// URL for downloading the file
    /// Computed property, not from entity
    /// </summary>
    public required string DownloadUrl { get; init; }

    /// <summary>
    /// Storage path of the uploaded file
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// URL for previewing the file (if applicable)
    /// </summary>
    public required string PreviewUrl { get; init; }
}
