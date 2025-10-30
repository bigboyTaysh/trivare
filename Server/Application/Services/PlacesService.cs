using Microsoft.Extensions.Logging;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Places;
using Trivare.Application.Interfaces;
using Trivare.Domain.Entities;
using Trivare.Domain.Interfaces;

namespace Trivare.Application.Services;

/// <summary>
/// Service implementation for AI-powered place search operations
/// Integrates Google Places API with OpenRouter.ai for intelligent place discovery
/// </summary>
public class PlacesService : IPlacesService
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IDayRepository _dayRepository;
    private readonly IPlaceRepository _placeRepository;
    private readonly IDayAttractionRepository _dayAttractionRepository;
    private readonly ILogger<PlacesService> _logger;
    // TODO: Add IGooglePlacesService interface and implementation in Infrastructure layer
    // TODO: Add IOpenRouterService interface and implementation in Infrastructure layer

    /// <summary>
    /// Initializes a new instance of the PlacesService
    /// </summary>
    /// <param name="auditLogRepository">Repository for audit log operations</param>
    /// <param name="dayRepository">Repository for day operations</param>
    /// <param name="placeRepository">Repository for place operations</param>
    /// <param name="dayAttractionRepository">Repository for day-attraction operations</param>
    /// <param name="logger">Logger instance</param>
    public PlacesService(
        IAuditLogRepository auditLogRepository,
        IDayRepository dayRepository,
        IPlaceRepository placeRepository,
        IDayAttractionRepository dayAttractionRepository,
        ILogger<PlacesService> logger)
    {
        _auditLogRepository = auditLogRepository;
        _dayRepository = dayRepository;
        _placeRepository = placeRepository;
        _dayAttractionRepository = dayAttractionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Searches for places using Google Places API with AI-powered filtering
    /// </summary>
    /// <param name="request">Search parameters including location, keyword, and optional preferences</param>
    /// <param name="userId">ID of the authenticated user performing the search</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search response with up to 5 filtered and ranked places</returns>
    public async Task<Result<PlaceSearchResponse>> SearchPlacesAsync(
        PlaceSearchRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Searching places for user {UserId} with location: {Location}, keyword: {Keyword}",
                userId, request.Location, request.Keyword);

            // TODO: Step 1 - Call Google Places API to fetch initial results
            // var googlePlacesResults = await _googlePlacesService.SearchAsync(
            //     request.Location, request.Keyword, cancellationToken);

            // TODO: Step 2 - Filter and rank results using OpenRouter.ai
            // var filteredResults = await _openRouterService.FilterAndRankPlacesAsync(
            //     googlePlacesResults, request.Preferences, cancellationToken);

            // TODO: Step 3 - Map results to PlaceDto objects
            // var placeDtos = filteredResults.Take(5).Select(MapToPlaceDto).ToList();

            // TODO: Step 4 - Log search event to AuditLog
            // await LogSearchEventAsync(userId, request, placeDtos.Count, cancellationToken);

            // TODO: Step 5 - Return response with results
            // return new PlaceSearchResponse
            // {
            //     Results = placeDtos,
            //     Count = placeDtos.Count
            // };

            // Temporary implementation - return empty results
            _logger.LogWarning("PlacesService.SearchPlacesAsync not fully implemented yet");
            
            await Task.CompletedTask; // Suppress async warning until full implementation
            
            return new PlaceSearchResponse
            {
                Results = Array.Empty<PlaceDto>(),
                Count = 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error searching places for user {UserId} with location: {Location}, keyword: {Keyword}",
                userId, request.Location, request.Keyword);
            
            return new ErrorResponse
            {
                Error = PlaceErrorCodes.PlaceSearchFailed,
                Message = "An error occurred while searching for places. Please try again later."
            };
        }
    }

    /// <summary>
    /// Logs a place search event to the audit log
    /// </summary>
    /// <param name="userId">ID of the user who performed the search</param>
    /// <param name="request">The search request parameters</param>
    /// <param name="resultCount">Number of results returned</param>
    /// <param name="cancellationToken">Cancellation token</param>
    private Task LogSearchEventAsync(
        Guid userId,
        PlaceSearchRequest request,
        int resultCount,
        CancellationToken cancellationToken)
    {
        // TODO: Implement audit log entry creation
        // var auditLog = new AuditLog
        // {
        //     UserId = userId,
        //     EventType = "PLACE_SEARCH",
        //     EventTimestamp = DateTime.UtcNow,
        //     Details = JsonSerializer.Serialize(new
        //     {
        //         Location = request.Location,
        //         Keyword = request.Keyword,
        //         Preferences = request.Preferences,
        //         ResultCount = resultCount
        //     })
        // };
        // 
        // await _auditLogRepository.AddAsync(auditLog, cancellationToken);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Adds a place to a specific day in the user's trip itinerary
    /// </summary>
    /// <param name="dayId">ID of the day to add the place to</param>
    /// <param name="request">Request containing place details and order</param>
    /// <param name="userId">ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the created day-attraction association</returns>
    public async Task<Result<DayAttractionDto>> AddPlaceToDayAsync(
        Guid dayId,
        AddPlaceRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Adding place to day {DayId} for user {UserId}",
                dayId, userId);

            // Input validation
            if (request.Order <= 0)
            {
                return new ErrorResponse
                {
                    Error = PlaceErrorCodes.InvalidOrder,
                    Message = "Order must be a positive integer"
                };
            }

            bool hasPlaceId = request.PlaceId.HasValue;
            bool hasPlace = request.Place is not null;

            if ((hasPlaceId && hasPlace) || (!hasPlaceId && !hasPlace))
            {
                return new ErrorResponse
                {
                    Error = PlaceErrorCodes.InvalidPlaceData,
                    Message = "Either PlaceId or Place must be provided, but not both"
                };
            }

            if (hasPlace)
            {
                if (string.IsNullOrWhiteSpace(request.Place!.Name))
                {
                    return new ErrorResponse
                    {
                        Error = PlaceErrorCodes.InvalidPlaceData,
                        Message = "Place name is required"
                    };
                }
                if (request.Place.Name.Length > 500) // assuming max length
                {
                    return new ErrorResponse
                    {
                        Error = PlaceErrorCodes.InvalidPlaceData,
                        Message = "Place name must be less than 500 characters"
                    };
                }
                // Add more validations for URL, etc.
            }

            // Get day with trip for ownership check
            var day = await _dayRepository.GetByIdWithTripAsync(dayId, cancellationToken);
            if (day == null)
            {
                return new ErrorResponse
                {
                    Error = PlaceErrorCodes.DayNotFound,
                    Message = "Day not found"
                };
            }

            if (day.Trip.UserId != userId)
            {
                return new ErrorResponse
                {
                    Error = PlaceErrorCodes.DayNotOwned,
                    Message = "Day does not belong to the authenticated user"
                };
            }

            // Resolve place
            Place? place;
            if (hasPlaceId)
            {
                place = await _placeRepository.GetByIdAsync(request.PlaceId!.Value, cancellationToken);
                if (place == null)
                {
                    return new ErrorResponse
                    {
                        Error = PlaceErrorCodes.PlaceNotFound,
                        Message = "Place not found"
                    };
                }
            }
            else
            {
                place = new Place
                {
                    Id = Guid.NewGuid(),
                    Name = request.Place!.Name.Trim(),
                    FormattedAddress = request.Place.FormattedAddress,
                    Website = request.Place.Website,
                    GoogleMapsLink = request.Place.GoogleMapsLink,
                    OpeningHoursText = request.Place.OpeningHoursText,
                    IsManuallyAdded = true
                };
                place = await _placeRepository.AddAsync(place, cancellationToken);
            }

            // Check if already added
            var existing = await _dayAttractionRepository.GetByDayIdAndPlaceIdAsync(dayId, place.Id, cancellationToken);
            if (existing != null)
            {
                return new ErrorResponse
                {
                    Error = PlaceErrorCodes.PlaceAlreadyAdded,
                    Message = "Place is already added to this day"
                };
            }

            // Create association
            var dayAttraction = new DayAttraction
            {
                DayId = dayId,
                PlaceId = place.Id,
                Order = request.Order,
                IsVisited = false
            };
            dayAttraction = await _dayAttractionRepository.AddAsync(dayAttraction, cancellationToken);

            // Map to DTO
            var placeDto = new PlaceDto
            {
                Id = place.Id,
                GooglePlaceId = place.GooglePlaceId,
                Name = place.Name,
                FormattedAddress = place.FormattedAddress,
                Website = place.Website,
                GoogleMapsLink = place.GoogleMapsLink,
                OpeningHoursText = place.OpeningHoursText,
                IsManuallyAdded = place.IsManuallyAdded
            };

            var result = new DayAttractionDto
            {
                DayId = dayAttraction.DayId,
                PlaceId = dayAttraction.PlaceId,
                Place = placeDto,
                Order = dayAttraction.Order,
                IsVisited = dayAttraction.IsVisited
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding place to day {DayId} for user {UserId}", dayId, userId);
            return new ErrorResponse
            {
                Error = PlaceErrorCodes.InternalServerError,
                Message = "An unexpected error occurred"
            };
        }
    }

    /// <summary>
    /// Updates the order or visited status of a place associated with a specific day
    /// </summary>
    /// <param name="dayId">ID of the day containing the place</param>
    /// <param name="placeId">ID of the place to update</param>
    /// <param name="request">Request containing fields to update</param>
    /// <param name="userId">ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the updated day-attraction data</returns>
    public async Task<Result<UpdateDayAttractionResponse>> UpdatePlaceOnDayAsync(
        Guid dayId,
        Guid placeId,
        UpdateDayAttractionRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Input validation
            if (request.Order.HasValue && request.Order.Value <= 0)
            {
                return new ErrorResponse
                {
                    Error = PlaceErrorCodes.InvalidOrder,
                    Message = "Order must be a positive integer"
                };
            }

            if (!request.Order.HasValue && !request.IsVisited.HasValue)
            {
                return new ErrorResponse
                {
                    Error = PlaceErrorCodes.NoFieldsToUpdate,
                    Message = "At least one field (order or isVisited) must be provided"
                };
            }

            // Get day with trip for ownership check
            var day = await _dayRepository.GetByIdWithTripAsync(dayId, cancellationToken);
            if (day == null)
            {
                return new ErrorResponse
                {
                    Error = PlaceErrorCodes.DayNotFound,
                    Message = "Day not found"
                };
            }

            if (day.Trip.UserId != userId)
            {
                return new ErrorResponse
                {
                    Error = PlaceErrorCodes.DayNotOwned,
                    Message = "Day does not belong to the authenticated user"
                };
            }

            // Get existing day-attraction
            var dayAttraction = await _dayAttractionRepository.GetByDayIdAndPlaceIdAsync(dayId, placeId, cancellationToken);
            if (dayAttraction == null)
            {
                return new ErrorResponse
                {
                    Error = PlaceErrorCodes.DayAttractionNotFound,
                    Message = "Place is not associated with this day"
                };
            }

            // Update fields
            if (request.Order.HasValue)
            {
                dayAttraction.Order = request.Order.Value;
            }
            if (request.IsVisited.HasValue)
            {
                dayAttraction.IsVisited = request.IsVisited.Value;
            }

            // Save changes
            dayAttraction = await _dayAttractionRepository.UpdateAsync(dayAttraction, cancellationToken);

            // Map to response
            var result = new UpdateDayAttractionResponse
            {
                DayId = dayAttraction.DayId,
                PlaceId = dayAttraction.PlaceId,
                Order = dayAttraction.Order,
                IsVisited = dayAttraction.IsVisited
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating place {PlaceId} on day {DayId} for user {UserId}", placeId, dayId, userId);
            return new ErrorResponse
            {
                Error = PlaceErrorCodes.InternalServerError,
                Message = "An unexpected error occurred"
            };
        }
    }

    /// <summary>
    /// Removes a place from a specific day in the user's trip itinerary
    /// </summary>
    /// <param name="dayId">ID of the day containing the place</param>
    /// <param name="placeId">ID of the place to remove</param>
    /// <param name="userId">ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure</returns>
    public async Task<Result<bool>> RemovePlaceFromDayAsync(
        Guid dayId,
        Guid placeId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation(
                "Removing place {PlaceId} from day {DayId} for user {UserId}",
                placeId, dayId, userId);

            // Get day with trip for ownership check
            var day = await _dayRepository.GetByIdWithTripAsync(dayId, cancellationToken);
            if (day == null)
            {
                return new ErrorResponse
                {
                    Error = PlaceErrorCodes.DayNotFound,
                    Message = "Day not found"
                };
            }

            if (day.Trip.UserId != userId)
            {
                return new ErrorResponse
                {
                    Error = PlaceErrorCodes.DayNotOwned,
                    Message = "Day does not belong to the authenticated user"
                };
            }

            // Get existing day-attraction
            var dayAttraction = await _dayAttractionRepository.GetByDayIdAndPlaceIdAsync(dayId, placeId, cancellationToken);
            if (dayAttraction == null)
            {
                return new ErrorResponse
                {
                    Error = PlaceErrorCodes.DayAttractionNotFound,
                    Message = "Place is not associated with this day"
                };
            }

            // Remove the association
            await _dayAttractionRepository.DeleteAsync(dayAttraction, cancellationToken);

            // Reorder remaining places
            var remainingAttractions = await _dayAttractionRepository.GetByDayIdAsync(dayId, cancellationToken);
            var remainingAttractionsList = remainingAttractions.OrderBy(da => da.Order).ToList();

            for (int i = 0; i < remainingAttractionsList.Count; i++)
            {
                if (remainingAttractionsList[i].Order != i + 1)
                {
                    remainingAttractionsList[i].Order = i + 1;
                    await _dayAttractionRepository.UpdateAsync(remainingAttractionsList[i], cancellationToken);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing place {PlaceId} from day {DayId} for user {UserId}", placeId, dayId, userId);
            return new ErrorResponse
            {
                Error = PlaceErrorCodes.InternalServerError,
                Message = "An unexpected error occurred"
            };
        }
    }

    /// <summary>
    /// Updates place details
    /// </summary>
    /// <param name="placeId">ID of the place to update</param>
    /// <param name="request">Request containing fields to update</param>
    /// <param name="userId">ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result containing the updated place data</returns>
    public async Task<Result<PlaceDto>> UpdatePlaceAsync(
        Guid placeId,
        UpdatePlaceRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Input validation - check if at least one field is provided
            if (string.IsNullOrWhiteSpace(request.Name) &&
                string.IsNullOrWhiteSpace(request.FormattedAddress) &&
                string.IsNullOrWhiteSpace(request.Website) &&
                string.IsNullOrWhiteSpace(request.GoogleMapsLink) &&
                string.IsNullOrWhiteSpace(request.OpeningHoursText))
            {
                return new ErrorResponse
                {
                    Error = PlaceErrorCodes.NoFieldsToUpdate,
                    Message = "At least one field must be provided for update"
                };
            }

            // Validate URL formats if provided
            if (!string.IsNullOrWhiteSpace(request.Website) &&
                !Uri.TryCreate(request.Website, UriKind.Absolute, out _))
            {
                return new ErrorResponse
                {
                    Error = PlaceErrorCodes.InvalidWebsiteUrl,
                    Message = "Website must be a valid URL"
                };
            }

            if (!string.IsNullOrWhiteSpace(request.GoogleMapsLink) &&
                !Uri.TryCreate(request.GoogleMapsLink, UriKind.Absolute, out _))
            {
                return new ErrorResponse
                {
                    Error = PlaceErrorCodes.InvalidGoogleMapsUrl,
                    Message = "Google Maps link must be a valid URL"
                };
            }

            // Get the place
            var place = await _placeRepository.GetByIdAsync(placeId, cancellationToken);
            if (place == null)
            {
                return new ErrorResponse
                {
                    Error = PlaceErrorCodes.PlaceNotFound,
                    Message = "Place not found"
                };
            }

            // Check user access - user must own at least one trip that contains this place
            var userHasAccess = await _dayAttractionRepository.UserHasAccessToPlaceAsync(userId, placeId, cancellationToken);
            if (!userHasAccess)
            {
                return new ErrorResponse
                {
                    Error = PlaceErrorCodes.PlaceNotOwned,
                    Message = "User does not have access to this place"
                };
            }

            // Update fields
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                place.Name = request.Name.Trim();
            }
            if (request.FormattedAddress != null)
            {
                place.FormattedAddress = string.IsNullOrWhiteSpace(request.FormattedAddress)
                    ? null
                    : request.FormattedAddress.Trim();
            }
            if (request.Website != null)
            {
                place.Website = string.IsNullOrWhiteSpace(request.Website)
                    ? null
                    : request.Website.Trim();
            }
            if (request.GoogleMapsLink != null)
            {
                place.GoogleMapsLink = string.IsNullOrWhiteSpace(request.GoogleMapsLink)
                    ? null
                    : request.GoogleMapsLink.Trim();
            }
            if (request.OpeningHoursText != null)
            {
                place.OpeningHoursText = string.IsNullOrWhiteSpace(request.OpeningHoursText)
                    ? null
                    : request.OpeningHoursText.Trim();
            }

            // Save changes
            place = await _placeRepository.UpdateAsync(place, cancellationToken);

            // Map to DTO
            var result = new PlaceDto
            {
                Id = place.Id,
                GooglePlaceId = place.GooglePlaceId,
                Name = place.Name,
                FormattedAddress = place.FormattedAddress,
                Website = place.Website,
                GoogleMapsLink = place.GoogleMapsLink,
                OpeningHoursText = place.OpeningHoursText,
                IsManuallyAdded = place.IsManuallyAdded
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating place {PlaceId} for user {UserId}", placeId, userId);
            return new ErrorResponse
            {
                Error = PlaceErrorCodes.InternalServerError,
                Message = "An unexpected error occurred"
            };
        }
    }
}
