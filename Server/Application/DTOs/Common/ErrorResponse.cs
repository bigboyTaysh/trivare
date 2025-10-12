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
    /// Additional error details (optional)
    /// </summary>
    public object? Details { get; init; }
}
