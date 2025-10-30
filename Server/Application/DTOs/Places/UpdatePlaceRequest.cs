namespace Trivare.Application.DTOs.Places;

/// <summary>
/// Request model for updating place details
/// Partial update of Place entity - all fields optional
/// </summary>
public record UpdatePlaceRequest
{
    /// <summary>
    /// Updated place name
    /// Updates Place.Name
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Updated formatted address
    /// Updates Place.FormattedAddress
    /// </summary>
    public string? FormattedAddress { get; init; }

    /// <summary>
    /// Updated website URL
    /// Updates Place.Website
    /// </summary>
    public string? Website { get; init; }

    /// <summary>
    /// Updated Google Maps link
    /// Updates Place.GoogleMapsLink
    /// </summary>
    public string? GoogleMapsLink { get; init; }

    /// <summary>
    /// Updated opening hours text
    /// Updates Place.OpeningHoursText
    /// </summary>
    public string? OpeningHoursText { get; init; }
}
