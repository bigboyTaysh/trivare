namespace Trivare.Domain.Interfaces;

/// <summary>
/// Service interface for Google Places API integration
/// Provides place search functionality using Google's Places API
/// </summary>
public interface IGooglePlacesService
{
    /// <summary>
    /// Searches for places using Google Places API based on location and keyword
    /// </summary>
    /// <param name="location">Location to search in (city, country, or coordinates)</param>
    /// <param name="keyword">Search keyword for type of place (e.g., "restaurants", "museums")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of places matching the search criteria</returns>
    Task<IEnumerable<GooglePlaceResult>> SearchPlacesAsync(
        string location,
        string keyword,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result model for Google Places API search
/// Contains raw data from Google Places API before AI filtering
/// </summary>
public record GooglePlaceResult
{
    /// <summary>
    /// Unique identifier from Google Places API
    /// </summary>
    public required string GooglePlaceId { get; init; }

    /// <summary>
    /// Name of the place
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Formatted address string
    /// </summary>
    public string? FormattedAddress { get; init; }

    /// <summary>
    /// Place website URL
    /// </summary>
    public string? Website { get; init; }

    /// <summary>
    /// Google Maps link for the place
    /// </summary>
    public string? GoogleMapsLink { get; init; }

    /// <summary>
    /// Opening hours as text
    /// </summary>
    public string? OpeningHoursText { get; init; }

    /// <summary>
    /// Place rating (0-5)
    /// </summary>
    public double? Rating { get; init; }

    /// <summary>
    /// Number of user ratings
    /// </summary>
    public int? UserRatingsTotal { get; init; }

    /// <summary>
    /// Place types/categories from Google
    /// </summary>
    public IEnumerable<string>? Types { get; init; }

    /// <summary>
    /// Price level indicator (0-4, where 0 is free and 4 is very expensive)
    /// </summary>
    public int? PriceLevel { get; init; }
}
