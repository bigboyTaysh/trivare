namespace Trivare.Application.DTOs.Places;

/// <summary>
/// Request model for adding a place to a day
/// Supports two modes: adding existing place by ID or creating new manual place
/// Maps to DayAttraction entity and optionally creates Place entity
/// </summary>
public record AddPlaceRequest
{
    /// <summary>
    /// ID of existing place to add
    /// Use this when adding a place from search results or previously created places
    /// Mutually exclusive with Place property
    /// </summary>
    public Guid? PlaceId { get; init; }

    /// <summary>
    /// New place data for manually creating a place
    /// Use this when adding a custom place not from Google Places API
    /// Mutually exclusive with PlaceId property
    /// </summary>
    public CreatePlaceRequest? Place { get; init; }

    /// <summary>
    /// Display order for the place in the day's itinerary
    /// From DayAttraction.Order
    /// </summary>
    public required int Order { get; init; }
}
