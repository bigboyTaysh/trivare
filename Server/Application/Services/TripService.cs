using Microsoft.Extensions.Logging;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Transport;
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

    /// <summary>
    /// Retrieves a paginated list of trips for the authenticated user
    /// Applies filtering, sorting, and pagination to the trip query
    /// </summary>
    public async Task<Result<TripListResponse>> GetTripsAsync(TripListRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Build query with filtering and sorting
        var (trips, totalItems) = await _tripRepository.GetTripsPaginatedAsync(userId, request.Search, request.SortBy, request.SortOrder, request.Page, request.PageSize, cancellationToken);

        // Map to DTOs
        var tripDtos = trips.Select(t => new TripListDto
        {
            Id = t.Id,
            Name = t.Name,
            Destination = t.Destination,
            StartDate = t.StartDate,
            EndDate = t.EndDate,
            Notes = t.Notes,
            CreatedAt = t.CreatedAt
        });

        // Calculate total pages
        var totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize);

        var pagination = new PaginationResponse
        {
            Page = request.Page,
            PageSize = request.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages
        };

        var response = new TripListResponse
        {
            Data = tripDtos,
            Pagination = pagination
        };

        return response;
    }

    /// <summary>
    /// Updates an existing trip for the authenticated user
    /// Validates input, checks ownership, applies partial updates, and logs the operation
    /// </summary>
    public async Task<Result<TripDetailDto>> UpdateTripAsync(Guid tripId, UpdateTripRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Validate input data
        if (request.Name != null && string.IsNullOrWhiteSpace(request.Name))
        {
            _logger.LogWarning("Trip update failed for user {UserId}: Invalid trip data - name is empty or whitespace", userId);
            return new ErrorResponse { Error = TripErrorCodes.InvalidTripData, Message = "Trip name cannot be empty or whitespace." };
        }

        // Validate date range if both dates are provided
        if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate.Value < request.StartDate.Value)
        {
            _logger.LogWarning("Trip update failed for user {UserId}: Invalid date range - end date before start date", userId);
            return new ErrorResponse { Error = TripErrorCodes.InvalidDateRange, Message = "End date must be on or after start date." };
        }

        // Fetch the trip with ownership check
        var trip = await _tripRepository.GetByIdAsync(tripId, cancellationToken);
        if (trip == null)
        {
            _logger.LogWarning("Trip update failed for user {UserId}: Trip {TripId} not found", userId, tripId);
            return new ErrorResponse { Error = TripErrorCodes.TripNotFound, Message = "Trip not found." };
        }

        if (trip.UserId != userId)
        {
            _logger.LogWarning("Trip update failed for user {UserId}: Trip {TripId} not owned by user", userId, tripId);
            return new ErrorResponse { Error = TripErrorCodes.TripNotOwned, Message = "Access denied." };
        }

        // Apply partial updates
        if (request.Name != null)
        {
            trip.Name = request.Name.Trim();
        }
        if (request.Destination != null)
        {
            trip.Destination = request.Destination.Trim();
        }
        if (request.StartDate.HasValue)
        {
            trip.StartDate = request.StartDate.Value;
        }
        if (request.EndDate.HasValue)
        {
            trip.EndDate = request.EndDate.Value;
        }
        if (request.Notes != null)
        {
            trip.Notes = request.Notes.Trim();
        }

        // Save changes
        await _tripRepository.UpdateAsync(trip, cancellationToken);

        // Map to response DTO
        var response = new TripDetailDto
        {
            Id = trip.Id,
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