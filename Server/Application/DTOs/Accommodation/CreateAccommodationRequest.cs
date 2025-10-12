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
    public string? Name { get; init; }

    /// <summary>
    /// Accommodation address - optional, max 500 characters
    /// </summary>
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
    public string? Notes { get; init; }
}
