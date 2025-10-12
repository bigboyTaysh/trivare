namespace Trivare.Application.DTOs.Places;

/// <summary>
/// Request model for AI-powered place search
/// Uses Google Places API with OpenRouter.ai filtering
/// </summary>
public record PlaceSearchRequest
{
    /// <summary>
    /// Location to search in (e.g., "Paris, France")
    /// </summary>
    public required string Location { get; init; }

    /// <summary>
    /// Search keyword for type of place (e.g., "restaurants", "museums")
    /// </summary>
    public required string Keyword { get; init; }

    /// <summary>
    /// User preferences for AI filtering (e.g., "vegetarian-friendly, outdoor seating")
    /// Optional
    /// </summary>
    public string? Preferences { get; init; }
}
