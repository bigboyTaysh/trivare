using System.ComponentModel.DataAnnotations;

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
    [Required(ErrorMessage = "Location is required")]
    [MaxLength(255, ErrorMessage = "Location cannot exceed 255 characters")]
    [MinLength(2, ErrorMessage = "Location must be at least 2 characters")]
    public required string Location { get; init; }

    /// <summary>
    /// Search keyword for type of place (e.g., "restaurants", "museums")
    /// </summary>
    [Required(ErrorMessage = "Keyword is required")]
    [MaxLength(255, ErrorMessage = "Keyword cannot exceed 255 characters")]
    [MinLength(2, ErrorMessage = "Keyword must be at least 2 characters")]
    public required string Keyword { get; init; }

    /// <summary>
    /// User preferences for AI filtering (e.g., "vegetarian-friendly, outdoor seating")
    /// Optional
    /// </summary>
    [MaxLength(500, ErrorMessage = "Preferences cannot exceed 500 characters")]
    public string? Preferences { get; init; }
}
