using System.ComponentModel.DataAnnotations;

namespace Trivare.Application.DTOs.Trips;

/// <summary>
/// Request model for creating a new trip
/// Maps to Trip entity excluding system-generated fields
/// </summary>
public record CreateTripRequest
{
    /// <summary>
    /// Trip name - required, minimum 1 character after trim
    /// </summary>
    [Required(ErrorMessage = "Trip name is required")]
    [MaxLength(255, ErrorMessage = "Trip name cannot exceed 255 characters")]
    public required string Name { get; init; }

    /// <summary>
    /// Trip destination - optional, max 255 characters
    /// </summary>
    [MaxLength(255, ErrorMessage = "Destination cannot exceed 255 characters")]
    public string? Destination { get; init; }

    /// <summary>
    /// Trip start date - required, can be in the past
    /// </summary>
    [Required(ErrorMessage = "Start date is required")]
    public required DateOnly StartDate { get; init; }

    /// <summary>
    /// Trip end date - required, must be >= StartDate
    /// </summary>
    [Required(ErrorMessage = "End date is required")]
    public required DateOnly EndDate { get; init; }

    /// <summary>
    /// Additional notes - optional, max 2000 characters
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; init; }
}
