using Microsoft.Extensions.Logging;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Days;
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
    private readonly ILogger<DayService> _logger;

    public DayService(IDayRepository dayRepository, ITripRepository tripRepository, ILogger<DayService> logger)
    {
        _dayRepository = dayRepository;
        _tripRepository = tripRepository;
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
            return new ErrorResponse { Error = "TripNotFound", Message = "The specified trip does not exist." };
        }

        if (trip.UserId != userId)
        {
            _logger.LogWarning("Day creation failed for user {UserId}: Trip {TripId} belongs to another user", userId, tripId);
            return new ErrorResponse { Error = "TripForbidden", Message = "You do not have permission to modify this trip." };
        }

        // Validate date is within trip range
        if (request.Date < trip.StartDate || request.Date > trip.EndDate)
        {
            _logger.LogWarning("Day creation failed for user {UserId}: Date {Date} is outside trip {TripId} range ({StartDate} to {EndDate})",
                userId, request.Date, tripId, trip.StartDate, trip.EndDate);
            return new ErrorResponse { Error = "InvalidDate", Message = "The date must be within the trip's start and end dates." };
        }

        // Check if day already exists for this trip and date
        var existingDay = await _dayRepository.GetByTripIdAndDateAsync(tripId, request.Date, cancellationToken);
        if (existingDay != null)
        {
            _logger.LogWarning("Day creation failed for user {UserId}: Day already exists for trip {TripId} on date {Date}", userId, tripId, request.Date);
            return new ErrorResponse { Error = "DayConflict", Message = "A day already exists for this trip on the specified date." };
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
}