namespace Trivare.Application.DTOs.Places;

/// <summary>
/// Response model for place search
/// Contains AI-filtered and ranked places from Google Places API
/// </summary>
public record PlaceSearchResponse
{
    /// <summary>
    /// Array of places matching search criteria
    /// Limited to top 5 results after AI filtering
    /// </summary>
    public required IEnumerable<PlaceDto> Results { get; init; }

    /// <summary>
    /// Number of results returned
    /// </summary>
    public required int Count { get; init; }
}
