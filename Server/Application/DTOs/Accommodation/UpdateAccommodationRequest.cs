using System.ComponentModel.DataAnnotations;

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
    [StringLength(255, ErrorMessage = "Name cannot exceed 255 characters.")]
    public string? Name { get; init; }

    /// <summary>
    /// Updated accommodation address - max 500 characters
    /// Updates Accommodation.Address
    /// </summary>
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters.")]
    public string? Address { get; init; }

    /// <summary>
    /// Updated check-in date - must be before CheckOutDate if both provided
    /// Updates Accommodation.CheckInDate
    /// </summary>
    [DataType(DataType.DateTime)]
    public DateTime? CheckInDate { get; init; }

    /// <summary>
    /// Updated check-out date - must be >= CheckInDate if both provided
    /// Updates Accommodation.CheckOutDate
    /// </summary>
    [DataType(DataType.DateTime)]
    public DateTime? CheckOutDate { get; init; }

    /// <summary>
    /// Updated notes - max 2000 characters
    /// Updates Accommodation.Notes
    /// </summary>
    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters.")]
    public string? Notes { get; init; }
}
