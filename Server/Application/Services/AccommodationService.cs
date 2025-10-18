using Microsoft.Extensions.Logging;
using Trivare.Application.DTOs.Accommodation;
using Trivare.Application.DTOs.Common;
using Trivare.Application.Interfaces;
using Trivare.Domain.Entities;
using Trivare.Domain.Interfaces;

namespace Trivare.Application.Services;

/// <summary>
/// Service implementation for accommodation-related operations
/// </summary>
public class AccommodationService : IAccommodationService
{
    private readonly IAccommodationRepository _accommodationRepository;
    private readonly ITripRepository _tripRepository;
    private readonly ILogger<AccommodationService> _logger;

    public AccommodationService(IAccommodationRepository accommodationRepository, ITripRepository tripRepository, ILogger<AccommodationService> logger)
    {
        _accommodationRepository = accommodationRepository;
        _tripRepository = tripRepository;
        _logger = logger;
    }

    /// <summary>
    /// Adds accommodation to a trip for the authenticated user
    /// Validates trip ownership, checks for existing accommodation, and validates dates
    /// </summary>
    public async Task<Result<AccommodationDto>> AddAccommodationAsync(CreateAccommodationRequest request, Guid tripId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Check if trip exists and belongs to user
        var trip = await _tripRepository.GetByIdAsync(tripId, cancellationToken);
        if (trip == null)
        {
            _logger.LogWarning("Accommodation addition failed for user {UserId}: Trip {TripId} not found", userId, tripId);
            return new ErrorResponse { Error = "TripNotFound", Message = "The specified trip does not exist." };
        }

        if (trip.UserId != userId)
        {
            _logger.LogWarning("Accommodation addition failed for user {UserId}: Trip {TripId} belongs to another user", userId, tripId);
            return new ErrorResponse { Error = "TripForbidden", Message = "You do not have permission to modify this trip." };
        }

        // Check if accommodation already exists for this trip
        var existingAccommodation = await _accommodationRepository.GetByTripIdAsync(tripId, cancellationToken);
        if (existingAccommodation != null)
        {
            _logger.LogWarning("Accommodation addition failed for user {UserId}: Accommodation already exists for trip {TripId}", userId, tripId);
            return new ErrorResponse { Error = "AccommodationAlreadyExists", Message = "An accommodation already exists for this trip." };
        }

        // Validate dates if both provided
        if (request.CheckInDate.HasValue && request.CheckOutDate.HasValue && request.CheckOutDate <= request.CheckInDate)
        {
            _logger.LogWarning("Accommodation addition failed for user {UserId}: Invalid date range for trip {TripId}", userId, tripId);
            return new ErrorResponse { Error = "InvalidDateRange", Message = "Check-out date must be after check-in date." };
        }

        // Create accommodation entity
        var accommodation = new Accommodation
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            Name = request.Name,
            Address = request.Address,
            CheckInDate = request.CheckInDate,
            CheckOutDate = request.CheckOutDate,
            Notes = request.Notes
        };

        // Add to repository
        var addedAccommodation = await _accommodationRepository.AddAsync(accommodation, cancellationToken);

        // Map to DTO
        var response = new AccommodationDto
        {
            Id = addedAccommodation.Id,
            TripId = addedAccommodation.TripId,
            Name = addedAccommodation.Name,
            Address = addedAccommodation.Address,
            CheckInDate = addedAccommodation.CheckInDate,
            CheckOutDate = addedAccommodation.CheckOutDate,
            Notes = addedAccommodation.Notes
        };

        return response;
    }
}