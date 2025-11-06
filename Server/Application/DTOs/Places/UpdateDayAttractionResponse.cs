namespace Trivare.Application.DTOs.Places;

/// <summary>
/// Response model for updating place-day association
/// Contains updated DayAttraction data after successful update
/// </summary>
public record UpdateDayAttractionResponse
{
    /// <summary>
    /// Parent day identifier
    /// </summary>
    public required Guid DayId { get; init; }

    /// <summary>
    /// Place identifier
    /// </summary>
    public required Guid PlaceId { get; init; }

    /// <summary>
    /// Updated display order in day's itinerary
    /// </summary>
    public required int Order { get; init; }

    /// <summary>
    /// Updated visited status
    /// </summary>
    public required bool IsVisited { get; init; }
}