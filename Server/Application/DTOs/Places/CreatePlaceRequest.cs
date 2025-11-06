namespace Trivare.Application.DTOs.Places;

/// <summary>
/// Request model for creating a new place
/// Maps to Place entity
/// Used when user adds custom attraction or place from search results
/// </summary>
public record CreatePlaceRequest
{
    /// <summary>
    /// Google Place ID from Google Places API
    /// Null for manually added places
    /// </summary>
    public string? GooglePlaceId { get; init; }

    /// <summary>
    /// Place name - required, minimum 1 character after trim
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Formatted address - optional, max 500 characters
    /// </summary>
    public string? FormattedAddress { get; init; }

    /// <summary>
    /// Place website - optional, must be valid URL
    /// </summary>
    public string? Website { get; init; }

    /// <summary>
    /// Google Maps link - optional, must be valid URL
    /// </summary>
    public string? GoogleMapsLink { get; init; }

    /// <summary>
    /// Opening hours text - optional, max 1000 characters
    /// </summary>
    public string? OpeningHoursText { get; init; }

    /// <summary>
    /// Google Places photo reference for fetching place images
    /// </summary>
    public string? PhotoReference { get; init; }
}
