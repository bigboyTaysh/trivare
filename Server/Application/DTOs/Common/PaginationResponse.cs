namespace Trivare.Application.DTOs.Common;

/// <summary>
/// Pagination metadata for list responses
/// </summary>
public record PaginationResponse
{
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
    public required int TotalItems { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public required int TotalPages { get; init; }
}
