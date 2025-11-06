using System.ComponentModel.DataAnnotations;

namespace Trivare.Application.DTOs.Trips;

/// <summary>
/// Request model for trip list parameters
/// </summary>
public record TripListRequest
{
    /// <summary>
    /// Page number for pagination (1-based, default: 1)
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0.")]
    public int Page { get; init; } = 1;

    /// <summary>
    /// Number of items per page (default: 10, max: 50)
    /// </summary>
    [Range(1, 50, ErrorMessage = "Page size must be between 1 and 50.")]
    public int PageSize { get; init; } = 10;

    /// <summary>
    /// Sort field (allowed: createdAt, name, startDate, default: createdAt)
    /// </summary>
    [RegularExpression("^(createdAt|name|startDate)$", ErrorMessage = "Sort field must be one of: createdAt, name, startDate.")]
    public string SortBy { get; init; } = "createdAt";

    /// <summary>
    /// Sort order (allowed: asc, desc, default: desc)
    /// </summary>
    [RegularExpression("^(asc|desc)$", ErrorMessage = "Sort order must be 'asc' or 'desc'.")]
    public string SortOrder { get; init; } = "desc";

    /// <summary>
    /// Search term for filtering by name, destination, notes, start date, or end date
    /// </summary>
    public string? Search { get; init; }
}