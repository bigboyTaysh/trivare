using Trivare.Domain.Interfaces;

namespace Trivare.Application.DTOs.Places;

/// <summary>
/// Response containing autocomplete predictions for places
/// </summary>
public class AutocompleteResponse
{
    /// <summary>
    /// List of autocomplete predictions
    /// </summary>
    public required List<AutocompletePredictionDto> Predictions { get; set; }
}

/// <summary>
/// DTO for autocomplete prediction
/// </summary>
public class AutocompletePredictionDto
{
    /// <summary>
    /// Description of the place (formatted address)
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Place ID for the prediction
    /// </summary>
    public required string PlaceId { get; set; }

    /// <summary>
    /// Structured formatting of the prediction
    /// </summary>
    public AutocompleteStructuredFormatDto? StructuredFormatting { get; set; }
}

/// <summary>
/// DTO for structured formatting of autocomplete predictions
/// </summary>
public class AutocompleteStructuredFormatDto
{
    /// <summary>
    /// Main text of the prediction (place name)
    /// </summary>
    public required string MainText { get; set; }

    /// <summary>
    /// Secondary text (address details)
    /// </summary>
    public required string SecondaryText { get; set; }
}
