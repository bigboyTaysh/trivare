namespace Trivare.Application.DTOs.Days;

/// <summary>
/// Request model for updating day information
/// Partial update of Day entity - all fields optional
/// </summary>
public record UpdateDayRequest
{
    /// <summary>
    /// Updated date - recommended to be within trip's date range
    /// Updates Day.Date
    /// </summary>
    public DateOnly? Date { get; init; }

    /// <summary>
    /// Updated notes - max 2000 characters
    /// Updates Day.Notes
    /// </summary>
    public string? Notes { get; init; }
}
