namespace Trivare.Application.DTOs.Places;

/// <summary>
/// Request model for creating a new manual place
/// Maps to Place entity with IsManuallyAdded = true
/// Used when user adds custom attraction not from Google Places API
/// </summary>
public record CreatePlaceRequest
{
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
}
