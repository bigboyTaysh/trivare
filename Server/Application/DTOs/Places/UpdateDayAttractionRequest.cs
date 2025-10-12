namespace Trivare.Application.DTOs.Places;

/// <summary>
/// Request model for updating place-day association
/// Partial update of DayAttraction entity - all fields optional
/// </summary>
public record UpdateDayAttractionRequest
{
    /// <summary>
    /// Updated display order in day's itinerary
    /// Updates DayAttraction.Order
    /// </summary>
    public int? Order { get; init; }

    /// <summary>
    /// Updated visited status
    /// Updates DayAttraction.IsVisited
    /// </summary>
    public bool? IsVisited { get; init; }
}
