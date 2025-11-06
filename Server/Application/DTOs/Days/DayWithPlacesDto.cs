using Trivare.Application.DTOs.Places;

namespace Trivare.Application.DTOs.Days;

/// <summary>
/// Day information with associated places
/// Derived from Day entity with DayAttractions and Places joined
/// </summary>
public record DayWithPlacesDto
{
    /// <summary>
    /// Day identifier from Day.Id
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Parent trip identifier from Day.TripId
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

    /// <summary>
    /// Places associated with this day
    /// Derived from Day.DayAttractions with Place details
    /// </summary>
    public required IEnumerable<DayAttractionDto> Places { get; init; }
}
