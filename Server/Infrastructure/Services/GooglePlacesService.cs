using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Trivare.Domain.Interfaces;
using Trivare.Infrastructure.Settings;
using System.Net.Http;

namespace Trivare.Infrastructure.Services;

/// <summary>
/// Service implementation for Google Places API integration
/// </summary>
public class GooglePlacesService : IGooglePlacesService
{
    private readonly GooglePlacesSettings _settings;
    private readonly ILogger<GooglePlacesService> _logger;
    private readonly HttpClient _httpClient;

    public GooglePlacesService(
        IOptions<GooglePlacesSettings> settings,
        IHttpClientFactory httpClientFactory,
        ILogger<GooglePlacesService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    /// <summary>
    /// Searches for places using Google Places API (New) - Text Search
    /// </summary>
    public async Task<List<GooglePlaceResult>> SearchPlacesAsync(
        string location,
        string keyword,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Build query combining keyword and location
            var textQuery = $"{keyword} in {location}";
            
            _logger.LogInformation("Searching Google Places (New API) with query: {Query}", textQuery);

            // Use Google Places API (New) - Text Search
            // Documentation: https://developers.google.com/maps/documentation/places/web-service/text-search
            var requestUrl = "https://places.googleapis.com/v1/places:searchText";

            var requestBody = new
            {
                textQuery = textQuery,
                maxResultCount = _settings.MaxResults,
                languageCode = "pl" // Use Polish for better results with Polish queries
            };

            var jsonContent = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var httpContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

            // Create request with required headers for new API
            var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = httpContent
            };
            request.Headers.Add("X-Goog-Api-Key", _settings.ApiKey);
            request.Headers.Add("X-Goog-FieldMask", "places.id,places.displayName,places.formattedAddress,places.rating,places.userRatingCount,places.types,places.regularOpeningHours,places.websiteUri,places.googleMapsUri,places.photos,places.priceLevel,places.businessStatus");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Google Places API (New) returned error {StatusCode}: {Response}",
                    response.StatusCode,
                    content);
                return new List<GooglePlaceResult>();
            }

            var result = JsonSerializer.Deserialize<GooglePlacesNewSearchResponse>(content, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (result?.Places == null || result.Places.Count == 0)
            {
                _logger.LogWarning(
                    "No results found for query: {Query}. Try using more specific location (e.g., 'City, Country') or different keywords.", 
                    textQuery);
                return new List<GooglePlaceResult>();
            }

            // Map new API response to our domain model
            var mappedPlaces = result.Places.Select(MapNewApiPlaceToResult).ToList();

            _logger.LogInformation("Found {Count} places for query: {Query}", mappedPlaces.Count, textQuery);
            return mappedPlaces;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching Google Places with location: {Location}, keyword: {Keyword}", 
                location, keyword);
            throw;
        }
    }

    /// <summary>
    /// Generates a Google Places Photo URL for a given photo reference
    /// Uses the new Google Places API (Place Photos)
    /// Documentation: https://developers.google.com/maps/documentation/places/web-service/place-photos
    /// </summary>
    /// <param name="photoReference">Photo reference from Google Places API (format: places/{place_id}/photos/{photo_id})</param>
    /// <param name="maxWidth">Maximum width of the photo (default: 400)</param>
    /// <returns>URL to fetch the photo directly from Google Places API</returns>
    public string GetPhotoUrl(string photoReference, int maxWidth = 400)
    {
        // For the new Google Places API, the photo reference format is: places/{place_id}/photos/{photo_id}
        // We need to use the Place Photos API: https://places.googleapis.com/v1/{photoReference}/media
        
        // Clean up the photo reference if needed
        var cleanReference = photoReference.TrimStart('/');
        
        // Build the photo URL with the API key
        return $"https://places.googleapis.com/v1/{cleanReference}/media?maxWidthPx={maxWidth}&key={_settings.ApiKey}";
    }

    /// <summary>
    /// Maps new API place to domain model
    /// </summary>
    private GooglePlaceResult MapNewApiPlaceToResult(GooglePlaceNew place)
    {
        // Extract photo references from photos array
        // Format from API: places/{place_id}/photos/{photo_id}
        var photoReferences = place.Photos?
            .Take(5) // Limit to 5 photos
            .Select(p => p.Name ?? string.Empty)
            .Where(p => !string.IsNullOrEmpty(p))
            .ToList() ?? new List<string>();

        return new GooglePlaceResult
        {
            PlaceId = place.Id ?? string.Empty,
            Name = place.DisplayName?.Text ?? "Unknown",
            FormattedAddress = place.FormattedAddress ?? string.Empty,
            Rating = place.Rating,
            UserRatingsTotal = place.UserRatingCount,
            Types = place.Types ?? new List<string>(),
            OpeningHoursText = place.RegularOpeningHours?.WeekdayDescriptions != null
                ? string.Join("; ", place.RegularOpeningHours.WeekdayDescriptions)
                : null,
            Website = place.WebsiteUri,
            GoogleMapsLink = place.GoogleMapsUri ?? $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(place.DisplayName?.Text ?? "")}&query_place_id={place.Id}",
            PhotoReferences = photoReferences,
            PriceLevel = place.PriceLevel switch
            {
                "PRICE_LEVEL_FREE" => 0,
                "PRICE_LEVEL_INEXPENSIVE" => 1,
                "PRICE_LEVEL_MODERATE" => 2,
                "PRICE_LEVEL_EXPENSIVE" => 3,
                "PRICE_LEVEL_VERY_EXPENSIVE" => 4,
                _ => null
            },
            BusinessStatus = place.BusinessStatus ?? "OPERATIONAL"
        };
    }

    /// <summary>
    /// Gets detailed information about a place using Google Places API Place Details (LEGACY - not used with new API)
    /// </summary>
    private async Task<GooglePlaceResult?> GetPlaceDetailsAsync(
        string placeId,
        CancellationToken cancellationToken)
    {
        try
        {
            var requestUrl = $"https://maps.googleapis.com/maps/api/place/details/json" +
                           $"?place_id={placeId}" +
                           $"&fields=place_id,name,formatted_address,rating,user_ratings_total,types," +
                           $"opening_hours,website,url,photos,price_level,business_status" +
                           $"&key={_settings.ApiKey}";

            var response = await _httpClient.GetAsync(requestUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var result = JsonSerializer.Deserialize<GooglePlaceDetailsResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (result?.Result == null)
            {
                return null;
            }

            var place = result.Result;
            
            // Build Google Maps link
            var googleMapsLink = $"https://www.google.com/maps/place/?q=place_id:{placeId}";

            // Extract photo references
            var photoReferences = place.Photos?
                .Select(p => p.PhotoReference)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList() ?? new List<string>();

            return new GooglePlaceResult
            {
                PlaceId = place.PlaceId,
                Name = place.Name,
                FormattedAddress = place.FormattedAddress,
                Rating = place.Rating,
                UserRatingsTotal = place.UserRatingsTotal,
                Types = place.Types ?? new List<string>(),
                OpeningHoursText = place.OpeningHours?.WeekdayText != null 
                    ? string.Join("; ", place.OpeningHours.WeekdayText) 
                    : null,
                Website = place.Website,
                GoogleMapsLink = googleMapsLink,
                PhotoReferences = photoReferences,
                PriceLevel = place.PriceLevel,
                BusinessStatus = place.BusinessStatus
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting details for place: {PlaceId}", placeId);
            return null;
        }
    }

    /// <summary>
    /// Gets place autocomplete predictions from Google Places API
    /// </summary>
    public async Task<List<AutocompletePrediction>> GetAutocompletePredictionsAsync(
        string input,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length < 3)
            {
                return new List<AutocompletePrediction>();
            }

            _logger.LogInformation("Getting autocomplete predictions for input: {Input}", input);

            // Use Google Places API (New) - Autocomplete
            // Documentation: https://developers.google.com/maps/documentation/places/web-service/place-autocomplete
            var requestUrl = "https://places.googleapis.com/v1/places:autocomplete";

            _logger.LogInformation("Making POST request to: {RequestUrl} with input: {Input}", requestUrl, input);

            // Create request body for Places API (New)
            var requestBody = new
            {
                input = input
            };

            var jsonContent = JsonSerializer.Serialize(requestBody);

            // Create a new HttpClient instance for this request to avoid header conflicts
            using var autocompleteHttpClient = new HttpClient();
            autocompleteHttpClient.DefaultRequestHeaders.Add("X-Goog-Api-Key", _settings.ApiKey);
            autocompleteHttpClient.DefaultRequestHeaders.Add("X-Goog-FieldMask", "suggestions.placePrediction.text,suggestions.placePrediction.placeId,suggestions.placePrediction.structuredFormat");

            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await autocompleteHttpClient.PostAsync(requestUrl, content, cancellationToken);

            _logger.LogInformation("Google Places API response status: {StatusCode}", response.StatusCode);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Google Places autocomplete API returned {StatusCode}", response.StatusCode);
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Error response: {ErrorContent}", errorContent);
                return new List<AutocompletePrediction>();
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Google Places API response: {JsonResponse}", jsonResponse);

            var autocompleteResponse = JsonSerializer.Deserialize<AutocompleteResponse>(jsonResponse, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (autocompleteResponse?.Suggestions == null)
            {
                _logger.LogInformation("No suggestions found in response");
                return new List<AutocompletePrediction>();
            }

            _logger.LogInformation("Found {Count} suggestions", autocompleteResponse.Suggestions.Count);

            return autocompleteResponse.Suggestions
                .Where(s => s.PlacePrediction != null)
                .Select(s => new AutocompletePrediction
                {
                    Description = s.PlacePrediction!.Text?.Text ?? "",
                    PlaceId = s.PlacePrediction!.PlaceId ?? "",
                    StructuredFormatting = s.PlacePrediction!.StructuredFormat != null ? new AutocompleteStructuredFormat
                    {
                        MainText = s.PlacePrediction!.StructuredFormat!.MainText?.Text ?? "",
                        SecondaryText = s.PlacePrediction!.StructuredFormat!.SecondaryText?.Text ?? ""
                    } : null
                })
                .Where(p => !string.IsNullOrEmpty(p.Description) && !string.IsNullOrEmpty(p.PlaceId))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting autocomplete predictions for input: {Input}", input);
            return new List<AutocompletePrediction>();
        }
    }

    #region Google Places API Response Models

    // New API Models
    private class GooglePlacesNewSearchResponse
    {
        public List<GooglePlaceNew>? Places { get; set; }
    }

    private class GooglePlaceNew
    {
        public string? Id { get; set; }
        public DisplayName? DisplayName { get; set; }
        public string? FormattedAddress { get; set; }
        public double? Rating { get; set; }
        public int? UserRatingCount { get; set; }
        public List<string>? Types { get; set; }
        public RegularOpeningHours? RegularOpeningHours { get; set; }
        public string? WebsiteUri { get; set; }
        public string? GoogleMapsUri { get; set; }
        public List<PhotoNew>? Photos { get; set; }
        public string? PriceLevel { get; set; }
        public string? BusinessStatus { get; set; }
    }

    private class DisplayName
    {
        public string? Text { get; set; }
        public string? LanguageCode { get; set; }
    }

    private class RegularOpeningHours
    {
        public List<string>? WeekdayDescriptions { get; set; }
    }

    private class PhotoNew
    {
        public string? Name { get; set; } // Format: places/{place_id}/photos/{photo_reference}
        public int? WidthPx { get; set; }
        public int? HeightPx { get; set; }
    }

    // Legacy API Models (kept for reference, not currently used)
    private class GooglePlacesTextSearchResponse
    {
        public List<GooglePlaceBasic> Results { get; set; } = new();
        public string? Status { get; set; }
    }

    private class GooglePlaceBasic
    {
        public string PlaceId { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    private class GooglePlaceDetailsResponse
    {
        public GooglePlaceDetails? Result { get; set; }
        public string? Status { get; set; }
    }

    private class GooglePlaceDetails
    {
        public string PlaceId { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string? FormattedAddress { get; set; }
        public double? Rating { get; set; }
        public int? UserRatingsTotal { get; set; }
        public List<string>? Types { get; set; }
        public OpeningHours? OpeningHours { get; set; }
        public string? Website { get; set; }
        public string? Url { get; set; }
        public List<Photo>? Photos { get; set; }
        public int? PriceLevel { get; set; }
        public string? BusinessStatus { get; set; }
    }

    private class OpeningHours
    {
        public List<string>? WeekdayText { get; set; }
    }

    private class Photo
    {
        public string PhotoReference { get; set; } = null!;
        public int Width { get; set; }
        public int Height { get; set; }
    }

    // Autocomplete API Models (New Places API)
    private class AutocompleteResponse
    {
        public List<AutocompleteSuggestion>? Suggestions { get; set; }
    }

    private class AutocompleteSuggestion
    {
        public PlacePrediction? PlacePrediction { get; set; }
    }

    private class PlacePrediction
    {
        public TextData? Text { get; set; }
        public string? PlaceId { get; set; }
        public StructuredFormatData? StructuredFormat { get; set; }
    }

    private class TextData
    {
        public string? Text { get; set; }
    }

    private class StructuredFormatData
    {
        public TextData? MainText { get; set; }
        public TextData? SecondaryText { get; set; }
    }

    #endregion
}

