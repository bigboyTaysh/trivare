using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Places;

namespace Trivare.Application.Interfaces;

/// <summary>
/// Service for AI-powered place search operations
/// Integrates Google Places API with OpenRouter.ai for intelligent filtering and ranking
/// </summary>
public interface IPlacesService
{
    /// <summary>
    /// Searches for places using Google Places API with AI-powered filtering
    /// Returns up to 5 relevant places based on location, keyword, and user preferences
    /// Logs search event to AuditLog for metrics tracking
    /// </summary>
    /// <param name="request">Search parameters including location, keyword, and optional preferences</param>
    /// <param name="userId">ID of the authenticated user performing the search</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search response with up to 5 filtered and ranked places</returns>
    Task<Result<PlaceSearchResponse>> SearchPlacesAsync(
        PlaceSearchRequest request, 
        Guid userId, 
        CancellationToken cancellationToken = default);
}
