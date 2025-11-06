namespace Trivare.Application.DTOs.Files;

/// <summary>
/// Data transfer object for file upload data
/// Abstracts away ASP.NET specific IFormFile for clean architecture
/// </summary>
public record FileUploadData
{
    /// <summary>
    /// The file stream
    /// </summary>
    public required Stream Content { get; init; }

    /// <summary>
    /// Original file name
    /// </summary>
    public required string FileName { get; init; }

    /// <summary>
    /// MIME content type
    /// </summary>
    public required string ContentType { get; init; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public required long Length { get; init; }
}