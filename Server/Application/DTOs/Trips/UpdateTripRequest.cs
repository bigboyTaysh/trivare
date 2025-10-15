using System.ComponentModel.DataAnnotations;

namespace Trivare.Application.DTOs.Trips;

/// <summary>
/// Request model for updating trip information
/// Partial update of Trip entity - all fields optional
/// </summary>
public record UpdateTripRequest
{
    /// <summary>
    /// Updated trip name - if provided, minimum 1 character after trim
    /// Updates Trip.Name
    /// </summary>
    public string? Name { get; init; }

    /// <summary>
    /// Updated destination - max 255 characters
    /// Updates Trip.Destination
    /// </summary>
    [StringLength(255, ErrorMessage = "Destination cannot exceed 255 characters")]
    public string? Destination { get; init; }

    /// <summary>
    /// Updated start date - must be <= EndDate if EndDate is also updated
    /// Updates Trip.StartDate
    /// </summary>
    public DateOnly? StartDate { get; init; }

    /// <summary>
    /// Updated end date - must be >= StartDate if StartDate is also updated
    /// Updates Trip.EndDate
    /// </summary>
    public DateOnly? EndDate { get; init; }

    /// <summary>
    /// Updated notes - max 2000 characters
    /// Updates Trip.Notes
    /// </summary>
    [StringLength(2000, ErrorMessage = "Notes cannot exceed 2000 characters")]
    public string? Notes { get; init; }
}
