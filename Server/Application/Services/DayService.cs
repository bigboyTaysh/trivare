using Microsoft.Extensions.Logging;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Days;
using Trivare.Application.DTOs.Places;
using Trivare.Application.Interfaces;
using Trivare.Domain.Entities;
using Trivare.Domain.Interfaces;

namespace Trivare.Application.Services;

/// <summary>
/// Service implementation for day-related operations
/// </summary>
public class DayService : IDayService
{
    private readonly IDayRepository _dayRepository;
    private readonly ITripRepository _tripRepository;
    private readonly IDayAttractionRepository _dayAttractionRepository;
    private readonly ILogger<DayService> _logger;

    public DayService(
        IDayRepository dayRepository, 
        ITripRepository tripRepository, 
        IDayAttractionRepository dayAttractionRepository,
        ILogger<DayService> logger)
    {
        _dayRepository = dayRepository;
        _tripRepository = tripRepository;
        _dayAttractionRepository = dayAttractionRepository;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new day for a trip
    /// Validates trip ownership, date range, and ensures no duplicate dates
    /// </summary>
    public async Task<Result<CreateDayResponse>> CreateDayAsync(CreateDayRequest request, Guid tripId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Check if trip exists and belongs to user
        var trip = await _tripRepository.GetByIdAsync(tripId, cancellationToken);
        if (trip == null)
        {
            _logger.LogWarning("Day creation failed for user {UserId}: Trip {TripId} not found", userId, tripId);
            return new ErrorResponse { Error = TripErrorCodes.TripNotFound, Message = "The specified trip does not exist." };
        }

        if (trip.UserId != userId)
        {
            _logger.LogWarning("Day creation failed for user {UserId}: Trip {TripId} belongs to another user", userId, tripId);
            return new ErrorResponse { Error = TripErrorCodes.TripNotOwned, Message = "You do not have permission to modify this trip." };
        }

        // Validate date is within trip range
        if (request.Date < trip.StartDate || request.Date > trip.EndDate)
        {
            _logger.LogWarning("Day creation failed for user {UserId}: Date {Date} is outside trip {TripId} range ({StartDate} to {EndDate})",
                userId, request.Date, tripId, trip.StartDate, trip.EndDate);
            return new ErrorResponse { Error = DayErrorCodes.InvalidDate, Message = "The date must be within the trip's start and end dates." };
        }

        // Check if day already exists for this trip and date
        var existingDay = await _dayRepository.GetByTripIdAndDateAsync(tripId, request.Date, cancellationToken);
        if (existingDay != null)
        {
            _logger.LogWarning("Day creation failed for user {UserId}: Day already exists for trip {TripId} on date {Date}", userId, tripId, request.Date);
            return new ErrorResponse { Error = DayErrorCodes.DayConflict, Message = "A day already exists for this trip on the specified date." };
        }

        // Create new day
        var day = new Day
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            Date = request.Date,
            Notes = request.Notes
        };

        var createdDay = await _dayRepository.AddAsync(day, cancellationToken);

        var response = new CreateDayResponse
        {
            Id = createdDay.Id,
            TripId = createdDay.TripId,
            Date = createdDay.Date,
            Notes = createdDay.Notes,
            CreatedAt = DateTime.UtcNow // Since entity doesn't have CreatedAt, use current time
        };

        _logger.LogInformation("Day {DayId} created for trip {TripId} by user {UserId}", createdDay.Id, tripId, userId);
        return response;
    }

    /// <summary>
    /// Gets all days for a specific trip with associated places
    /// Validates trip ownership before returning days
    /// </summary>
    public async Task<Result<IEnumerable<DayWithPlacesDto>>> GetDaysForTripAsync(Guid tripId, Guid userId, CancellationToken cancellationToken = default)
    {
        // Check if trip exists and belongs to user
        var trip = await _tripRepository.GetByIdAsync(tripId, cancellationToken);
        if (trip == null)
        {
            _logger.LogWarning("Days retrieval failed for user {UserId}: Trip {TripId} not found", userId, tripId);
            return new ErrorResponse { Error = TripErrorCodes.TripNotFound, Message = "The specified trip does not exist." };
        }

        if (trip.UserId != userId)
        {
            _logger.LogWarning("Days retrieval failed for user {UserId}: Trip {TripId} belongs to another user", userId, tripId);
            return new ErrorResponse { Error = TripErrorCodes.TripNotOwned, Message = "You do not have permission to access this trip." };
        }

        // Get days for the trip
        var days = await _dayRepository.GetByTripIdAsync(tripId, cancellationToken);

        var dayWithPlacesDtos = new List<DayWithPlacesDto>();

        foreach (var day in days)
        {
            // Get places for this day
            var dayAttractions = await _dayAttractionRepository.GetByDayIdAsync(day.Id, cancellationToken);

            var placeDtos = dayAttractions.Select(da => new DayAttractionDto
            {
                DayId = da.DayId,
                PlaceId = da.PlaceId,
                Place = new PlaceDto
                {
                    Id = da.Place.Id,
                    GooglePlaceId = da.Place.GooglePlaceId,
                    Name = da.Place.Name,
                    FormattedAddress = da.Place.FormattedAddress,
                    Website = da.Place.Website,
                    GoogleMapsLink = da.Place.GoogleMapsLink,
                    OpeningHoursText = da.Place.OpeningHoursText,
                    PhotoReference = da.Place.PhotoReference, // Raw reference without API key
                    IsManuallyAdded = da.Place.IsManuallyAdded
                },
                Order = da.Order,
                IsVisited = da.IsVisited
            });

            var dayWithPlacesDto = new DayWithPlacesDto
            {
                Id = day.Id,
                TripId = day.TripId,
                Date = day.Date,
                Notes = day.Notes,
                Places = placeDtos
            };

            dayWithPlacesDtos.Add(dayWithPlacesDto);
        }

        return dayWithPlacesDtos;
    }

    /// <summary>
    /// Updates an existing day
    /// Validates day ownership and ensures no duplicate dates within the trip
    /// </summary>
    public async Task<Result<DayDto>> UpdateDayAsync(Guid dayId, UpdateDayRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Check if day exists
        var day = await _dayRepository.GetByIdAsync(dayId, cancellationToken);
        if (day == null)
        {
            _logger.LogWarning("Day update failed for user {UserId}: Day {DayId} not found", userId, dayId);
            return new ErrorResponse { Error = DayErrorCodes.DayNotFound, Message = "The specified day does not exist." };
        }

        // Check if trip exists and belongs to user
        var trip = await _tripRepository.GetByIdAsync(day.TripId, cancellationToken);
        if (trip == null)
        {
            _logger.LogWarning("Day update failed for user {UserId}: Trip {TripId} not found", userId, day.TripId);
            return new ErrorResponse { Error = TripErrorCodes.TripNotFound, Message = "The trip associated with this day does not exist." };
        }

        if (trip.UserId != userId)
        {
            _logger.LogWarning("Day update failed for user {UserId}: Day {DayId} belongs to another user", userId, dayId);
            return new ErrorResponse { Error = DayErrorCodes.DayForbidden, Message = "You do not have permission to modify this day." };
        }

        // Validate date if provided
        if (request.Date.HasValue)
        {
            // Check if date is within trip range
            if (request.Date.Value < trip.StartDate || request.Date.Value > trip.EndDate)
            {
                _logger.LogWarning("Day update failed for user {UserId}: Date {Date} is outside trip {TripId} range ({StartDate} to {EndDate})",
                    userId, request.Date.Value, day.TripId, trip.StartDate, trip.EndDate);
                return new ErrorResponse { Error = DayErrorCodes.InvalidDate, Message = "The date must be within the trip's start and end dates." };
            }

            // Check if another day already exists for this trip and date (excluding current day)
            var existingDay = await _dayRepository.GetByTripIdAndDateAsync(day.TripId, request.Date.Value, cancellationToken);
            if (existingDay != null && existingDay.Id != dayId)
            {
                _logger.LogWarning("Day update failed for user {UserId}: Another day already exists for trip {TripId} on date {Date}", userId, day.TripId, request.Date.Value);
                return new ErrorResponse { Error = DayErrorCodes.DayConflict, Message = "Another day already exists for this trip on the specified date." };
            }

            day.Date = request.Date.Value;
        }

        // Update notes if provided
        if (request.Notes != null)
        {
            day.Notes = request.Notes;
        }

        var updatedDay = await _dayRepository.UpdateAsync(day, cancellationToken);

        var response = new DayDto
        {
            Id = updatedDay.Id,
            TripId = updatedDay.TripId,
            Date = updatedDay.Date,
            Notes = updatedDay.Notes
        };

        return response;
    }
}