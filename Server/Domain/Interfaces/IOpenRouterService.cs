using Trivare.Domain.DTOs.OpenRouter;

namespace Trivare.Domain.Interfaces;

/// <summary>
/// Service interface for OpenRouter AI chat completion API
/// Provides both generic structured JSON responses and specialized place filtering
/// </summary>
public interface IOpenRouterService
{
    /// <summary>
    /// Sends a chat completion request to OpenRouter API and expects a structured JSON response
    /// </summary>
    /// <typeparam name="TResponse">The type to deserialize the response into</typeparam>
    /// <param name="userMessage">The user's prompt</param>
    /// <param name="systemMessage">Instructions for the LLM's behavior and response format</param>
    /// <param name="modelName">The identifier for the LLM to use (e.g., openai/gpt-4o)</param>
    /// <param name="schema">The JSON schema for the expected response structure</param>
    /// <param name="cancellationToken">Token for cancelling the asynchronous operation</param>
    /// <returns>An instance of TResponse, or null if the operation fails or returns no content</returns>
    Task<TResponse?> GetStructuredChatCompletionAsync<TResponse>(
        string userMessage,
        string systemMessage,
        string modelName,
        JsonSchema schema,
        CancellationToken cancellationToken = default) where TResponse : class;

    /// <summary>
    /// Filters and ranks places using AI based on user preferences
    /// </summary>
    /// <param name="places">List of places from Google Places API</param>
    /// <param name="userPreferences">User search preferences/query</param>
    /// <param name="location">Location context</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Filtered and ranked list of places</returns>
    Task<List<GooglePlaceResult>> FilterAndRankPlacesAsync(
        List<GooglePlaceResult> places,
        string userPreferences,
        string location,
        CancellationToken cancellationToken = default);
}
