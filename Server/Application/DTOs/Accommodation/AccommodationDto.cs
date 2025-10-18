namespace Trivare.Application.DTOs.Accommodation;

/// <summary>
/// Accommodation details data transfer object
/// Derived from Accommodation entity
/// </summary>
public record AccommodationDto
{
    /// <summary>
    /// Accommodation identifier from Accommodation.Id
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Parent trip identifier from Accommodation.TripId
    /// </summary>
    public required Guid TripId { get; init; }

    /// <summary>
    /// Accommodation name from Accommodation.Name
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Accommodation address from Accommodation.Address
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// Check-in date and time from Accommodation.CheckInDate
    /// </summary>
    public DateTime? CheckInDate { get; init; }

    /// <summary>
    /// Check-out date and time from Accommodation.CheckOutDate
    /// </summary>
    public DateTime? CheckOutDate { get; init; }

    /// <summary>
    /// Additional notes from Accommodation.Notes
    /// </summary>
    public string? Notes { get; init; }
}
