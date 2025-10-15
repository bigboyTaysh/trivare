using Microsoft.Extensions.Logging;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Trips;
using Trivare.Application.Interfaces;
using Trivare.Domain.Entities;
using Trivare.Domain.Interfaces;

namespace Trivare.Application.Services;

/// <summary>
/// Service implementation for trip-related operations
/// </summary>
public class TripService : ITripService
{
    private readonly ITripRepository _tripRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<TripService> _logger;

    public TripService(ITripRepository tripRepository, IAuditLogRepository auditLogRepository, ILogger<TripService> logger)
    {
        _tripRepository = tripRepository;
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new trip for the authenticated user
    /// Validates input, enforces business rules, and logs the operation
    /// </summary>
    public async Task<Result<CreateTripResponse>> CreateTripAsync(CreateTripRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Validate business rules
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogWarning("Trip creation failed for user {UserId}: Invalid trip data - name is empty or whitespace", userId);
            return new ErrorResponse { Error = TripErrorCodes.InvalidTripData, Message = "Trip name cannot be empty or whitespace." };
        }

        if (request.EndDate < request.StartDate)
        {
            _logger.LogWarning("Trip creation failed for user {UserId}: Invalid date range - end date before start date", userId);
            return new ErrorResponse { Error = TripErrorCodes.InvalidDateRange, Message = "End date must be on or after start date." };
        }

        // Check trip limit
        var tripCount = await _tripRepository.CountByUserIdAsync(userId, cancellationToken);
        if (tripCount >= 10)
        {
            _logger.LogWarning("Trip creation failed for user {UserId}: Trip limit exceeded ({Count}/10)", userId, tripCount);
            return new ErrorResponse { Error = TripErrorCodes.TripLimitExceeded, Message = "Maximum of 10 trips per user exceeded." };
        }

        // Create trip entity
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name.Trim(),
            Destination = request.Destination?.Trim(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Notes = request.Notes?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        // Save to database
        await _tripRepository.AddAsync(trip, cancellationToken);

        // Log audit event
        var auditLog = new AuditLog
        {
            UserId = userId,
            EventType = "TripCreated",
            EventTimestamp = DateTime.UtcNow,
            Details = $"Trip {trip.Id} created"
        };
        await _auditLogRepository.AddAsync(auditLog, cancellationToken);

        // Map to response
        var response = new CreateTripResponse
        {
            Id = trip.Id,
            UserId = trip.UserId,
            Name = trip.Name,
            Destination = trip.Destination,
            StartDate = trip.StartDate,
            EndDate = trip.EndDate,
            Notes = trip.Notes,
            CreatedAt = trip.CreatedAt
        };

        return response;
    }
}