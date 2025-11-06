using Trivare.Application.DTOs.Common;

namespace Trivare.Application.DTOs.Trips;

/// <summary>
/// Response model for trip list with pagination
/// </summary>
public record TripListResponse
{
    /// <summary>
    /// Array of trip list DTOs
    /// </summary>
    public required IEnumerable<TripListDto> Data { get; init; }

    /// <summary>
    /// Pagination metadata
    /// </summary>
    public required PaginationResponse Pagination { get; init; }
}