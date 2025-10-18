namespace Trivare.Domain.Interfaces;

/// <summary>
/// Service interface for OpenRouter.ai integration
/// Provides AI-powered filtering and ranking of places based on user preferences
/// </summary>
public interface IOpenRouterService
{
    /// <summary>
    /// Filters and ranks places using AI based on user preferences
    /// Sends place data to OpenRouter.ai for intelligent analysis and ranking
    /// </summary>
    /// <param name="places">List of places from Google Places API to filter</param>
    /// <param name="preferences">User preferences for filtering (e.g., "vegetarian-friendly, outdoor seating")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Filtered and ranked list of places (up to 5 results)</returns>
    Task<IEnumerable<GooglePlaceResult>> FilterAndRankPlacesAsync(
        IEnumerable<GooglePlaceResult> places,
        string? preferences,
        CancellationToken cancellationToken = default);
}
