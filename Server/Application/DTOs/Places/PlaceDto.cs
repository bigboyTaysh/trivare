namespace Trivare.Application.DTOs.Places;

/// <summary>
/// Place (attraction) data transfer object
/// Derived from Place entity
/// </summary>
public record PlaceDto
{
    /// <summary>
    /// Place identifier from Place.Id
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Google Places API identifier from Place.GooglePlaceId
    /// Null for manually added places
    /// </summary>
    public string? GooglePlaceId { get; init; }

    /// <summary>
    /// Place name from Place.Name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Formatted address from Place.FormattedAddress
    /// </summary>
    public string? FormattedAddress { get; init; }

    /// <summary>
    /// Place website from Place.Website
    /// </summary>
    public string? Website { get; init; }

    /// <summary>
    /// Google Maps link from Place.GoogleMapsLink
    /// </summary>
    public string? GoogleMapsLink { get; init; }

    /// <summary>
    /// Opening hours text from Place.OpeningHoursText
    /// </summary>
    public string? OpeningHoursText { get; init; }

    /// <summary>
    /// Google Places photo reference for fetching place images
    /// From Place.PhotoReference
    /// </summary>
    public string? PhotoReference { get; init; }

    /// <summary>
    /// Indicates if place was manually added vs from Google Places API
    /// From Place.IsManuallyAdded
    /// </summary>
    public required bool IsManuallyAdded { get; init; }
}
