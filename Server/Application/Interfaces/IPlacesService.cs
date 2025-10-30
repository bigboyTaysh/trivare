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

    /// <summary>
    /// Adds a place to a specific day in the user's trip itinerary
    /// Supports adding existing places or creating new manual places
    /// Validates ownership, prevents duplicates, and maintains proper ordering
    /// </summary>
    /// <param name="dayId">ID of the day to add the place to</param>
    /// <param name="request">Request containing place details and order</param>
    /// <param name="userId">ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the created day-attraction association</returns>
    Task<Result<DayAttractionDto>> AddPlaceToDayAsync(
        Guid dayId,
        AddPlaceRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the order or visited status of a place associated with a specific day
    /// Supports partial updates - only provided fields are updated
    /// Validates ownership and ensures the association exists
    /// </summary>
    /// <param name="dayId">ID of the day containing the place</param>
    /// <param name="placeId">ID of the place to update</param>
    /// <param name="request">Request containing fields to update (order and/or isVisited)</param>
    /// <param name="userId">ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the updated day-attraction data</returns>
    Task<Result<UpdateDayAttractionResponse>> UpdatePlaceOnDayAsync(
        Guid dayId,
        Guid placeId,
        UpdateDayAttractionRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a place from a specific day in the user's trip itinerary
    /// Validates ownership and ensures the association exists
    /// Reorders remaining places to maintain proper sequence
    /// </summary>
    /// <param name="dayId">ID of the day containing the place</param>
    /// <param name="placeId">ID of the place to remove</param>
    /// <param name="userId">ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    Task<Result<bool>> RemovePlaceFromDayAsync(
        Guid dayId,
        Guid placeId,
        Guid userId,
        CancellationToken cancellationToken = default);
}
