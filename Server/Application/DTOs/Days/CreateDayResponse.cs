namespace Trivare.Application.DTOs.Days;

/// <summary>
/// Response model for creating a day
/// </summary>
public record CreateDayResponse
{
    /// <summary>
    /// The unique identifier of the created day
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The identifier of the trip this day belongs to
    /// </summary>
    public required Guid TripId { get; init; }

    /// <summary>
    /// The date of the day
    /// </summary>
    public required DateOnly Date { get; init; }

    /// <summary>
    /// Additional notes for the day
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// The timestamp when the day was created
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}