using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trivare.Domain.DTOs.OpenRouter;
using Trivare.Domain.Interfaces;
using Trivare.Infrastructure.Settings;

namespace Trivare.Infrastructure.Services;

/// <summary>
/// Service implementation for communicating with OpenRouter AI API
/// Provides structured JSON responses from Large Language Models
/// </summary>
public class OpenRouterService : IOpenRouterService
{
    private readonly HttpClient _httpClient;
    private readonly OpenRouterSettings _settings;
    private readonly ILogger<OpenRouterService> _logger;
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = false
    };

    public OpenRouterService(
        IHttpClientFactory httpClientFactory,
        IOptions<OpenRouterSettings> settings,
        ILogger<OpenRouterService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("OpenRouterClient");

        // Configure HttpClient instance
        // Ensure BaseUrl ends with slash for proper relative URI resolution
        var baseUrl = _settings.BaseUrl.TrimEnd('/') + "/";
        _httpClient.BaseAddress = new Uri(baseUrl);
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://trivare.app");
        _httpClient.DefaultRequestHeaders.Add("X-Title", "Trivare");
    }

    /// <summary>
    /// Sends a structured chat completion request to OpenRouter API
    /// </summary>
    public async Task<TResponse?> GetStructuredChatCompletionAsync<TResponse>(
        string userMessage,
        string systemMessage,
        string modelName,
        JsonSchema schema,
        CancellationToken cancellationToken = default) where TResponse : class
    {
        // Validate inputs
        if (string.IsNullOrWhiteSpace(userMessage))
        {
            _logger.LogWarning("User message is null or empty");
            return null;
        }

        if (string.IsNullOrWhiteSpace(systemMessage))
        {
            _logger.LogWarning("System message is null or empty");
            return null;
        }

        if (string.IsNullOrWhiteSpace(modelName))
        {
            _logger.LogWarning("Model name is null or empty");
            return null;
        }

        if (schema == null)
        {
            _logger.LogWarning("Schema is null");
            return null;
        }

        try
        {
            // Build request payload
            var request = new OpenRouterChatRequest(
                Model: modelName,
                Messages: new List<ChatMessage>
                {
                    new("system", systemMessage),
                    new("user", userMessage)
                },
                ResponseFormat: new ResponseFormat(
                    Type: "json_schema",
                    JsonSchema: new JsonSchemaPayload(
                        Name: $"get_{typeof(TResponse).Name.ToLowerInvariant()}",
                        Strict: true,
                        Schema: schema
                    )
                )
            );

            _logger.LogInformation(
                "Sending structured chat completion request to OpenRouter. Model: {Model}, ResponseType: {ResponseType}",
                modelName,
                typeof(TResponse).Name);

            // Send request
            var response = await SendRequestAsync(request, cancellationToken);

            if (response == null)
            {
                _logger.LogWarning("OpenRouter API returned null response");
                return null;
            }

            // Extract content from response
            var content = response.Choices?.FirstOrDefault()?.Message?.Content;

            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("OpenRouter API returned empty content");
                return null;
            }

            _logger.LogDebug("Received response content: {Content}", content);

            // Deserialize to target type
            var result = JsonSerializer.Deserialize<TResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result == null)
            {
                _logger.LogWarning("Failed to deserialize response to {Type}", typeof(TResponse).Name);
                return null;
            }

            _logger.LogInformation(
                "Successfully completed structured chat request. Tokens used: {TotalTokens}",
                response.Usage?.TotalTokens ?? 0);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error while calling OpenRouter API: {Message}", ex.Message);
            return null;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            _logger.LogError(ex, "Request to OpenRouter API timed out");
            return null;
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("OpenRouter API request was cancelled");
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error: {Message}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during OpenRouter API call: {Message}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Sends an HTTP POST request to the OpenRouter API
    /// </summary>
    private async Task<OpenRouterChatCompletionResponse?> SendRequestAsync(
        OpenRouterChatRequest requestBody,
        CancellationToken cancellationToken)
    {
        var content = SerializeRequest(requestBody);

        try
        {
            var response = await _httpClient.PostAsync("chat/completions", content, cancellationToken);

            // Check for API errors (4xx/5xx)
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "OpenRouter API returned error status {StatusCode}: {ErrorContent}",
                    response.StatusCode,
                    errorContent);

                // Throw custom exception for API errors
                throw new HttpRequestException(
                    $"OpenRouter API returned {response.StatusCode}: {errorContent}");
            }

            // Deserialize response
            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
            var apiResponse = JsonSerializer.Deserialize<OpenRouterChatCompletionResponse>(
                responseContent,
                _serializerOptions);

            return apiResponse;
        }
        catch (HttpRequestException)
        {
            // Re-throw HTTP exceptions to be handled by caller
            throw;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize OpenRouter API response");
            throw;
        }
    }

    /// <summary>
    /// Serializes the request body to JSON StringContent
    /// </summary>
    private static StringContent SerializeRequest(OpenRouterChatRequest requestBody)
    {
        var jsonRequest = JsonSerializer.Serialize(requestBody, _serializerOptions);
        return new StringContent(jsonRequest, Encoding.UTF8, "application/json");
    }

    #region Legacy Implementation for Backward Compatibility
    
    /// <summary>
    /// Filters and ranks places using AI (legacy implementation for backward compatibility)
    /// This method implements the Domain.Interfaces.IOpenRouterService interface
    /// TODO: Migrate PlacesService to use GetStructuredChatCompletionAsync instead
    /// </summary>
    public async Task<List<GooglePlaceResult>> FilterAndRankPlacesAsync(
        List<GooglePlaceResult> places,
        string userPreferences,
        string location,
        CancellationToken cancellationToken = default)
    {
        if (places.Count == 0)
        {
            return places;
        }

        try
        {
            _logger.LogInformation(
                "Filtering {Count} places with AI for preferences: {Preferences}", 
                places.Count, userPreferences);

            // Build prompt for AI
            var prompt = BuildFilteringPrompt(places, userPreferences, location);

            // Build simple request without structured schema (legacy approach)
            // Note: Not all models support response_format, so we'll parse the response manually
            var requestBodyJson = new
            {
                model = _settings.Model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                }
            };
            
            var jsonContent = JsonSerializer.Serialize(requestBodyJson, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
            });

            var fullUrl = new Uri(_httpClient.BaseAddress!, "chat/completions").ToString();
            _logger.LogDebug(
                "Sending request to OpenRouter. URL: {Url}, Model: {Model}",
                fullUrl,
                _settings.Model);

            var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("chat/completions", httpContent, cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            // Check for errors before deserializing
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "OpenRouter API returned error status {StatusCode}. Response: {Response}",
                    response.StatusCode,
                    responseContent);
                    
                // Return fallback results
                return places
                    .OrderByDescending(p => p.Rating ?? 0)
                    .ThenByDescending(p => p.UserRatingsTotal ?? 0)
                    .Take(_settings.ResultCount)
                    .ToList();
            }

            // Log response for debugging if it doesn't look like JSON
            if (!responseContent.TrimStart().StartsWith("{"))
            {
                _logger.LogError(
                    "OpenRouter API returned non-JSON response. Response starts with: {ResponseStart}",
                    responseContent.Length > 200 ? responseContent.Substring(0, 200) : responseContent);
                    
                // Return fallback results
                return places
                    .OrderByDescending(p => p.Rating ?? 0)
                    .ThenByDescending(p => p.UserRatingsTotal ?? 0)
                    .Take(_settings.ResultCount)
                    .ToList();
            }

            var result = JsonSerializer.Deserialize<OpenRouterChatCompletionResponse>(
                responseContent,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result?.Choices == null || result.Choices.Count == 0)
            {
                _logger.LogWarning("No AI response received, returning top rated places");
                return places
                    .OrderByDescending(p => p.Rating ?? 0)
                    .ThenByDescending(p => p.UserRatingsTotal ?? 0)
                    .Take(_settings.ResultCount)
                    .ToList();
            }

            // Parse AI response to get selected place IDs
            var aiResponse = result.Choices[0].Message.Content;
            var selectedPlaceIds = ParseAIResponse(aiResponse);

            // Filter and reorder places based on AI selection
            var filteredPlaces = new List<GooglePlaceResult>();
            foreach (var placeId in selectedPlaceIds)
            {
                var place = places.FirstOrDefault(p => p.PlaceId == placeId);
                if (place != null)
                {
                    filteredPlaces.Add(place);
                }

                if (filteredPlaces.Count >= _settings.ResultCount)
                {
                    break;
                }
            }

            _logger.LogInformation("AI filtered to {Count} places", filteredPlaces.Count);
            return filteredPlaces;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error filtering places with AI, returning top rated places");
            // Fallback: return top-rated places
            return places
                .OrderByDescending(p => p.Rating ?? 0)
                .ThenByDescending(p => p.UserRatingsTotal ?? 0)
                .Take(_settings.ResultCount)
                .ToList();
        }
    }

    private string BuildFilteringPrompt(
        List<GooglePlaceResult> places,
        string userPreferences,
        string location)
    {
        var placesJson = places.Select((p, index) => new
        {
            index = index + 1,
            placeId = p.PlaceId,
            name = p.Name,
            address = p.FormattedAddress,
            rating = p.Rating,
            ratingsCount = p.UserRatingsTotal,
            types = p.Types,
            priceLevel = p.PriceLevel,
            businessStatus = p.BusinessStatus
        });

        var placesJsonString = JsonSerializer.Serialize(placesJson, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        return $@"You are a travel expert helping users find the best places to visit. 

User is looking for: ""{userPreferences}""
Location: {location}

Here are {places.Count} places from Google Places API:

{placesJsonString}

Task: Select the best {Math.Min(_settings.ResultCount, places.Count)} places that match the user's preferences.

Consider:
1. Relevance to user preferences
2. Rating and number of reviews
3. Business status (prefer OPERATIONAL)
4. Variety and uniqueness
5. Price level appropriateness

Respond with ONLY a JSON array of selected place IDs in order of preference, like this:
[""place_id_1"", ""place_id_2"", ""place_id_3"", ...]

Do not include any explanation, just the JSON array.";
    }

    private List<string> ParseAIResponse(string aiResponse)
    {
        try
        {
            // Remove markdown code blocks if present
            var json = aiResponse.Trim();
            if (json.StartsWith("```"))
            {
                var lines = json.Split('\n');
                json = string.Join('\n', lines.Skip(1).SkipLast(1));
            }

            var placeIds = JsonSerializer.Deserialize<List<string>>(json);
            return placeIds ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing AI response: {Response}", aiResponse);
            return new List<string>();
        }
    }
    
    #endregion
}
