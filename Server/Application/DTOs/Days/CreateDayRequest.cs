namespace Trivare.Application.DTOs.Days;

/// <summary>
/// Request model for adding a day to a trip
/// Maps to Day entity excluding Id and TripId (provided in route)
/// </summary>
public record CreateDayRequest
{
    /// <summary>
    /// Date for the new day - required
    /// Recommended to be within trip's StartDate and EndDate
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Additional notes for the day - optional, max 2000 characters
    /// </summary>
    public string? Notes { get; init; }
}
