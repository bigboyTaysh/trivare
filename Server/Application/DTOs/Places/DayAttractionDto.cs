namespace Trivare.Application.DTOs.Places;

/// <summary>
/// Day-place association data transfer object
/// Derived from DayAttraction entity with Place details
/// Represents a place added to a specific day's itinerary
/// </summary>
public record DayAttractionDto
{
    /// <summary>
    /// Parent day identifier from DayAttraction.DayId
    /// </summary>
    public required Guid DayId { get; init; }

    /// <summary>
    /// Place identifier from DayAttraction.PlaceId
    /// </summary>
    public required Guid PlaceId { get; init; }

    /// <summary>
    /// Full place details from DayAttraction.Place
    /// </summary>
    public required PlaceDto Place { get; init; }

    /// <summary>
    /// Display order in day's itinerary from DayAttraction.Order
    /// </summary>
    public required int Order { get; init; }

    /// <summary>
    /// Whether the place has been visited from DayAttraction.IsVisited
    /// </summary>
    public required bool IsVisited { get; init; }
}
