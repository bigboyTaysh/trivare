namespace Trivare.Application.DTOs.Common;

/// <summary>
/// Standard error response structure for API errors
/// </summary>
public record ErrorResponse
{
    /// <summary>
    /// Error code identifier
    /// </summary>
    public required string Error { get; init; }

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Validation errors grouped by field name (optional)
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; init; }
}
