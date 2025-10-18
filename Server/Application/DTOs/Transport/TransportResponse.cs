namespace Trivare.Application.DTOs.Transport;

/// <summary>
/// Response model for transport operations
/// Derived from Transport entity
/// </summary>
public record TransportResponse
{
    /// <summary>
    /// Transport identifier from Transport.Id
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public required Guid Id { get; init; }

    /// <summary>
    /// Parent trip identifier from Transport.TripId
    /// </summary>
    /// <example>123e4567-e89b-12d3-a456-426614174000</example>
    public required Guid TripId { get; init; }

    /// <summary>
    /// Type of transportation from Transport.Type
    /// </summary>
    /// <example>Flight</example>
    public required string Type { get; init; }

    /// <summary>
    /// Departure location from Transport.DepartureLocation
    /// </summary>
    /// <example>John F. Kennedy International Airport (JFK)</example>
    public string? DepartureLocation { get; init; }

    /// <summary>
    /// Arrival location from Transport.ArrivalLocation
    /// </summary>
    /// <example>Charles de Gaulle Airport (CDG)</example>
    public string? ArrivalLocation { get; init; }

    /// <summary>
    /// Departure date and time from Transport.DepartureTime
    /// </summary>
    /// <example>2024-12-15T14:30:00Z</example>
    public DateTime? DepartureTime { get; init; }

    /// <summary>
    /// Arrival date and time from Transport.ArrivalTime
    /// </summary>
    /// <example>2024-12-15T18:45:00Z</example>
    public DateTime? ArrivalTime { get; init; }

    /// <summary>
    /// Additional notes from Transport.Notes
    /// </summary>
    /// <example>Direct flight with meal service. Gate changes possible.</example>
    public string? Notes { get; init; }
}