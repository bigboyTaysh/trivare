using System.ComponentModel.DataAnnotations;

namespace Trivare.Application.DTOs.Transport;

/// <summary>
/// Request model for updating transportation details
/// Partial update of Transport entity - all fields optional
/// </summary>
public record UpdateTransportRequest
{
    /// <summary>
    /// Updated transportation type - max 100 characters
    /// Updates Transport.Type
    /// </summary>
    [StringLength(100, ErrorMessage = "Type cannot exceed 100 characters.")]
    public string? Type { get; init; }

    /// <summary>
    /// Updated departure location - max 255 characters
    /// Updates Transport.DepartureLocation
    /// </summary>
    [StringLength(255, ErrorMessage = "Departure location cannot exceed 255 characters.")]
    public string? DepartureLocation { get; init; }

    /// <summary>
    /// Updated arrival location - max 255 characters
    /// Updates Transport.ArrivalLocation
    /// </summary>
    [StringLength(255, ErrorMessage = "Arrival location cannot exceed 255 characters.")]
    public string? ArrivalLocation { get; init; }

    /// <summary>
    /// Updated departure time - must be before ArrivalTime if both provided
    /// Updates Transport.DepartureTime
    /// </summary>
    public DateTime? DepartureTime { get; init; }

    /// <summary>
    /// Updated arrival time - must be after DepartureTime if both provided
    /// Updates Transport.ArrivalTime
    /// </summary>
    public DateTime? ArrivalTime { get; init; }

    /// <summary>
    /// Updated notes - max 2000 characters
    /// Updates Transport.Notes
    /// </summary>
    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters.")]
    public string? Notes { get; init; }
}
