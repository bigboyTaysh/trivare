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

    /// <summary>
    /// Updates accommodation for a trip owned by the authenticated user
    /// Supports partial updates - only provided fields are modified
    /// Validates trip ownership and date logic if both dates provided
    /// </summary>
    public async Task<Result<AccommodationDto>> UpdateAccommodationAsync(UpdateAccommodationRequest request, Guid tripId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Check if trip exists and belongs to user
        var trip = await _tripRepository.GetByIdAsync(tripId, cancellationToken);
        if (trip == null)
        {
            _logger.LogWarning("Accommodation update failed for user {UserId}: Trip {TripId} not found", userId, tripId);
            return new ErrorResponse { Error = TripErrorCodes.TripNotFound, Message = "The specified trip does not exist." };
        }

        if (trip.UserId != userId)
        {
            _logger.LogWarning("Accommodation update failed for user {UserId}: Trip {TripId} belongs to another user", userId, tripId);
            return new ErrorResponse { Error = TripErrorCodes.TripNotOwned, Message = "You do not have permission to modify this trip." };
        }

        // Get existing accommodation
        var accommodation = await _accommodationRepository.GetByTripIdAsync(tripId, cancellationToken);
        if (accommodation == null)
        {
            _logger.LogWarning("Accommodation update failed for user {UserId}: Accommodation not found for trip {TripId}", userId, tripId);
            return new ErrorResponse { Error = AccommodationErrorCodes.AccommodationNotFound, Message = "No accommodation found for this trip." };
        }

        // Validate dates if both provided in request
        DateTime? newCheckIn = request.CheckInDate ?? accommodation.CheckInDate;
        DateTime? newCheckOut = request.CheckOutDate ?? accommodation.CheckOutDate;
        if (newCheckIn.HasValue && newCheckOut.HasValue && newCheckOut <= newCheckIn)
        {
            _logger.LogWarning("Accommodation update failed for user {UserId}: Invalid date range for trip {TripId}", userId, tripId);
            return new ErrorResponse { Error = AccommodationErrorCodes.AccommodationInvalidDateRange, Message = "Check-out date must be after check-in date." };
        }

        // Apply partial updates
        if (request.Name != null) accommodation.Name = request.Name;
        if (request.Address != null) accommodation.Address = request.Address;
        if (request.CheckInDate.HasValue) accommodation.CheckInDate = request.CheckInDate;
        if (request.CheckOutDate.HasValue) accommodation.CheckOutDate = request.CheckOutDate;
        if (request.Notes != null) accommodation.Notes = request.Notes;

        // Update in repository
        var updatedAccommodation = await _accommodationRepository.UpdateAsync(accommodation, cancellationToken);

        // Map to DTO
        var response = new AccommodationDto
        {
            Id = updatedAccommodation.Id,
            TripId = updatedAccommodation.TripId,
            Name = updatedAccommodation.Name,
            Address = updatedAccommodation.Address,
            CheckInDate = updatedAccommodation.CheckInDate,
            CheckOutDate = updatedAccommodation.CheckOutDate,
            Notes = updatedAccommodation.Notes
        };

        return response;
    }
}