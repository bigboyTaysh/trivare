namespace Trivare.Domain.Interfaces;

/// <summary>
/// Service for interacting with Google Places API
/// </summary>
public interface IGooglePlacesService
{
    /// <summary>
    /// Searches for places near a location using Google Places API
    /// </summary>
    /// <param name="location">Location (city, address, or coordinates)</param>
    /// <param name="keyword">Search keyword (e.g., "museum", "breakfast with coffee")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of places from Google Places API</returns>
    Task<List<GooglePlaceResult>> SearchPlacesAsync(
        string location,
        string keyword,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result from Google Places API search
/// </summary>
public class GooglePlaceResult
{
    /// <summary>
    /// Google Place ID
    /// </summary>
    public required string PlaceId { get; set; }

    /// <summary>
    /// Place name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Formatted address
    /// </summary>
    public string? FormattedAddress { get; set; }

    /// <summary>
    /// Place rating (0-5)
    /// </summary>
    public double? Rating { get; set; }

    /// <summary>
    /// Number of ratings
    /// </summary>
    public int? UserRatingsTotal { get; set; }

    /// <summary>
    /// Place types (e.g., "restaurant", "museum")
    /// </summary>
    public List<string> Types { get; set; } = new();

    /// <summary>
    /// Opening hours text
    /// </summary>
    public string? OpeningHoursText { get; set; }

    /// <summary>
    /// Website URL
    /// </summary>
    public string? Website { get; set; }

    /// <summary>
    /// Google Maps link
    /// </summary>
    public string? GoogleMapsLink { get; set; }

    /// <summary>
    /// Photo references from Google Places
    /// </summary>
    public List<string> PhotoReferences { get; set; } = new();

    /// <summary>
    /// Price level (0-4)
    /// </summary>
    public int? PriceLevel { get; set; }

    /// <summary>
    /// Business status
    /// </summary>
    public string? BusinessStatus { get; set; }
}
