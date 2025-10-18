using System.ComponentModel.DataAnnotations;

namespace Trivare.Application.DTOs.Accommodation;

/// <summary>
/// Request model for adding accommodation to a trip
/// Maps to Accommodation entity excluding Id and TripId (provided in route)
/// </summary>
public record CreateAccommodationRequest
{
    /// <summary>
    /// Accommodation name - optional, max 255 characters
    /// </summary>
    [MaxLength(255, ErrorMessage = "Accommodation name cannot exceed 255 characters")]
    public string? Name { get; init; }

    /// <summary>
    /// Accommodation address - optional, max 500 characters
    /// </summary>
    [MaxLength(500, ErrorMessage = "Accommodation address cannot exceed 500 characters")]
    public string? Address { get; init; }

    /// <summary>
    /// Check-in date and time - optional, must be before CheckOutDate if both provided
    /// </summary>
    public DateTime? CheckInDate { get; init; }

    /// <summary>
    /// Check-out date and time - optional, must be >= CheckInDate if both provided
    /// </summary>
    public DateTime? CheckOutDate { get; init; }

    /// <summary>
    /// Additional notes - optional, max 2000 characters
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; init; }
}
