using System.ComponentModel.DataAnnotations;

namespace Trivare.Application.DTOs.Transport;

/// <summary>
/// Request model for adding transportation to a trip
/// Maps to Transport entity excluding Id and TripId (provided in route)
/// </summary>
public record CreateTransportRequest
{
    /// <summary>
    /// Type of transportation - required, max 100 characters
    /// </summary>
    /// <example>Flight</example>
    [Required(ErrorMessage = "Type is required.")]
    [StringLength(100, ErrorMessage = "Type cannot exceed 100 characters.")]
    public required string Type { get; init; }

    /// <summary>
    /// Departure location - optional, max 255 characters
    /// </summary>
    /// <example>John F. Kennedy International Airport (JFK)</example>
    [StringLength(255, ErrorMessage = "Departure location cannot exceed 255 characters.")]
    public string? DepartureLocation { get; init; }

    /// <summary>
    /// Arrival location - optional, max 255 characters
    /// </summary>
    /// <example>Charles de Gaulle Airport (CDG)</example>
    [StringLength(255, ErrorMessage = "Arrival location cannot exceed 255 characters.")]
    public string? ArrivalLocation { get; init; }

    /// <summary>
    /// Departure date and time - optional, must be before ArrivalTime if both provided
    /// </summary>
    /// <example>2024-12-15T14:30:00Z</example>
    public DateTime? DepartureTime { get; init; }

    /// <summary>
    /// Arrival date and time - optional, must be after DepartureTime if both provided
    /// </summary>
    /// <example>2024-12-15T18:45:00Z</example>
    public DateTime? ArrivalTime { get; init; }

    /// <summary>
    /// Additional notes - optional, max 2000 characters
    /// </summary>
    /// <example>Direct flight with meal service. Gate changes possible.</example>
    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters.")]
    public string? Notes { get; init; }
}
