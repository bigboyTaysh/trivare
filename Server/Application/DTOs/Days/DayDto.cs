namespace Trivare.Application.DTOs.Days;

/// <summary>
/// Day information data transfer object
/// Derived from Day entity without nested places
/// </summary>
public record DayDto
{
    /// <summary>
    /// Day identifier from Day.Id
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Trip identifier from Day.TripId
    /// </summary>
    public required Guid TripId { get; init; }

    /// <summary>
    /// Date for this day from Day.Date
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Additional notes from Day.Notes
    /// </summary>
    public string? Notes { get; init; }
}
