namespace Trivare.Application.DTOs.Transport;

/// <summary>
/// Request model for adding transportation to a trip
/// Maps to Transport entity excluding Id and TripId (provided in route)
/// </summary>
public record CreateTransportRequest
{
    /// <summary>
    /// Type of transportation - optional, max 100 characters
    /// </summary>
    public string? Type { get; init; }

    /// <summary>
    /// Departure location - optional, max 255 characters
    /// </summary>
    public string? DepartureLocation { get; init; }

    /// <summary>
    /// Arrival location - optional, max 255 characters
    /// </summary>
    public string? ArrivalLocation { get; init; }

    /// <summary>
    /// Departure date and time - optional, must be before ArrivalTime if both provided
    /// </summary>
    public DateTime? DepartureTime { get; init; }

    /// <summary>
    /// Arrival date and time - optional, must be after DepartureTime if both provided
    /// </summary>
    public DateTime? ArrivalTime { get; init; }

    /// <summary>
    /// Additional notes - optional, max 2000 characters
    /// </summary>
    public string? Notes { get; init; }
}
