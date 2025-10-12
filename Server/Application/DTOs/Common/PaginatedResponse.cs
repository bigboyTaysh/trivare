namespace Trivare.Application.DTOs.Common;

/// <summary>
/// Generic pagination wrapper for list responses
/// </summary>
/// <typeparam name="T">Type of items in the data array</typeparam>
public record PaginatedResponse<T>
{
    /// <summary>
    /// Array of items for the current page
    /// </summary>
    public required IEnumerable<T> Data { get; init; }

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public required int Page { get; init; }

    /// <summary>
    /// Number of items per page
    /// </summary>
    public required int PageSize { get; init; }

    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public required int TotalCount { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public required int TotalPages { get; init; }
}
