namespace Trivare.Application.DTOs.Trips;

/// <summary>
/// Request model for creating a new trip
/// Maps to Trip entity excluding system-generated fields
/// </summary>
public record CreateTripRequest
{
    /// <summary>
    /// Trip name - required, minimum 1 character after trim
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Trip destination - optional, max 255 characters
    /// </summary>
    public string? Destination { get; init; }

    /// <summary>
    /// Trip start date - required, can be in the past
    /// </summary>
    public required DateOnly StartDate { get; init; }

    /// <summary>
    /// Trip end date - required, must be >= StartDate
    /// </summary>
    public required DateOnly EndDate { get; init; }

    /// <summary>
    /// Additional notes - optional, max 2000 characters
    /// </summary>
    public string? Notes { get; init; }
}
