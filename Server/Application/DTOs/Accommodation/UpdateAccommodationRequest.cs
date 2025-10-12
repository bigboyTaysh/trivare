namespace Trivare.Application.DTOs.Accommodation;

/// <summary>
/// Request model for updating accommodation details
/// Partial update of Accommodation entity - all fields optional
/// </summary>
public record UpdateAccommodationRequest
{
    /// <summary>
    /// Updated accommodation name - max 255 characters
    /// Updates Accommodation.Name
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Updated accommodation address - max 500 characters
    /// Updates Accommodation.Address
    /// </summary>
    public string? Address { get; init; }

    /// <summary>
    /// Updated check-in date - must be before CheckOutDate if both provided
    /// Updates Accommodation.CheckInDate
    /// </summary>
    public DateTime? CheckInDate { get; init; }

    /// <summary>
    /// Updated check-out date - must be >= CheckInDate if both provided
    /// Updates Accommodation.CheckOutDate
    /// </summary>
    public DateTime? CheckOutDate { get; init; }

    /// <summary>
    /// Updated notes - max 2000 characters
    /// Updates Accommodation.Notes
    /// </summary>
    public string? Notes { get; init; }
}
