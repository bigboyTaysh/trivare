namespace Trivare.Application.DTOs.Transport;

/// <summary>
/// Transportation details data transfer object
/// Derived from Transport entity
/// </summary>
public record TransportDto
{
    /// <summary>
    /// Transport identifier from Transport.Id
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Parent trip identifier from Transport.TripId
    /// </summary>
    public required Guid TripId { get; init; }

    /// <summary>
    /// Type of transportation from Transport.Type
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Departure location from Transport.DepartureLocation
    /// </summary>
    public string? DepartureLocation { get; init; }

    /// <summary>
    /// Arrival location from Transport.ArrivalLocation
    /// </summary>
    public string? ArrivalLocation { get; init; }

    /// <summary>
    /// Departure date and time from Transport.DepartureTime
    /// </summary>
    public DateTime? DepartureTime { get; init; }

    /// <summary>
    /// Arrival date and time from Transport.ArrivalTime
    /// </summary>
    public DateTime? ArrivalTime { get; init; }

    /// <summary>
    /// Additional notes from Transport.Notes
    /// </summary>
    public string? Notes { get; init; }
}
