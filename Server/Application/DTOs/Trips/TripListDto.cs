namespace Trivare.Application.DTOs.Trips;

/// <summary>
/// Simplified trip data for list views
/// Derived from Trip entity without nested relations
/// </summary>
public record TripListDto
{
    /// <summary>
    /// Trip identifier from Trip.Id
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Trip name from Trip.Name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Trip destination from Trip.Destination
    /// </summary>
    public string? Destination { get; init; }

    /// <summary>
    /// Trip start date from Trip.StartDate
    /// </summary>
    public required DateOnly StartDate { get; init; }

    /// <summary>
    /// Trip end date from Trip.EndDate
    /// </summary>
    public required DateOnly EndDate { get; init; }

    /// <summary>
    /// Additional notes from Trip.Notes
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Trip creation timestamp from Trip.CreatedAt
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
