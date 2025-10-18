using Microsoft.Extensions.Logging;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Transport;
using Trivare.Application.Interfaces;
using Trivare.Domain.Entities;
using Trivare.Domain.Interfaces;

namespace Trivare.Application.Services;

/// <summary>
/// Service implementation for transport-related operations
/// </summary>
public class TransportService : ITransportService
{
    private readonly ITransportRepository _transportRepository;
    private readonly ITripRepository _tripRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ILogger<TransportService> _logger;

    public TransportService(ITransportRepository transportRepository, ITripRepository tripRepository, IAuditLogRepository auditLogRepository, ILogger<TransportService> logger)
    {
        _transportRepository = transportRepository;
        _tripRepository = tripRepository;
        _auditLogRepository = auditLogRepository;
        _logger = logger;
    }

    /// <summary>
    /// Creates transportation details for an existing trip
    /// Validates input, enforces business rules, and logs the operation
    /// </summary>
    public async Task<Result<CreateTransportResponse>> CreateTransportAsync(Guid tripId, CreateTransportRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Validate trip exists and belongs to user
        var trip = await _tripRepository.GetByIdAsync(tripId, cancellationToken);
        if (trip == null)
        {
            _logger.LogWarning("Transport creation failed for user {UserId}: Trip {TripId} not found", userId, tripId);
            return new ErrorResponse { Error = "TripNotFound", Message = "Trip not found." };
        }

        if (trip.UserId != userId)
        {
            _logger.LogWarning("Transport creation failed for user {UserId}: Trip {TripId} belongs to another user", userId, tripId);
            return new ErrorResponse { Error = "TripForbidden", Message = "Trip belongs to another user." };
        }

        // Validate business rules
        if (request.DepartureTime.HasValue && request.ArrivalTime.HasValue && request.ArrivalTime.Value <= request.DepartureTime.Value)
        {
            _logger.LogWarning("Transport creation failed for user {UserId}: Invalid time range - arrival before or at departure", userId);
            return new ErrorResponse { Error = "InvalidTimeRange", Message = "Arrival time must be after departure time." };
        }

        // Create transport entity
        var transport = new Transport
        {
            Id = Guid.NewGuid(),
            TripId = tripId,
            Type = request.Type.Trim(),
            DepartureLocation = request.DepartureLocation?.Trim(),
            ArrivalLocation = request.ArrivalLocation?.Trim(),
            DepartureTime = request.DepartureTime,
            ArrivalTime = request.ArrivalTime,
            Notes = request.Notes?.Trim()
        };

        // Save to database
        await _transportRepository.AddAsync(transport, cancellationToken);

        // Map to response
        var response = new CreateTransportResponse
        {
            Id = transport.Id,
            TripId = transport.TripId,
            Type = transport.Type,
            DepartureLocation = transport.DepartureLocation,
            ArrivalLocation = transport.ArrivalLocation,
            DepartureTime = transport.DepartureTime,
            ArrivalTime = transport.ArrivalTime,
            Notes = transport.Notes
        };

        return response;
    }

    /// <summary>
    /// Updates transportation details for an existing transport record
    /// Validates ownership, enforces business rules, and logs the operation
    /// </summary>
    public async Task<Result<TransportResponse>> UpdateTransportAsync(Guid transportId, UpdateTransportRequest request, Guid userId, CancellationToken cancellationToken = default)
    {
        // Get transport and validate it exists
        var transport = await _transportRepository.GetByIdAsync(transportId, cancellationToken);
        if (transport == null)
        {
            _logger.LogWarning("Transport update failed for user {UserId}: Transport {TransportId} not found", userId, transportId);
            return new ErrorResponse { Error = "TransportNotFound", Message = "Transport not found." };
        }

        // Validate trip ownership
        var trip = await _tripRepository.GetByIdAsync(transport.TripId, cancellationToken);
        if (trip == null || trip.UserId != userId)
        {
            _logger.LogWarning("Transport update failed for user {UserId}: Transport {TransportId} belongs to another user", userId, transportId);
            return new ErrorResponse { Error = "TransportForbidden", Message = "Transport belongs to another user." };
        }

        // Validate business rules for provided times
        var departureTime = request.DepartureTime ?? transport.DepartureTime;
        var arrivalTime = request.ArrivalTime ?? transport.ArrivalTime;
        if (departureTime.HasValue && arrivalTime.HasValue && arrivalTime.Value <= departureTime.Value)
        {
            _logger.LogWarning("Transport update failed for user {UserId}: Invalid time range - arrival before or at departure", userId);
            return new ErrorResponse { Error = "InvalidTimeRange", Message = "Arrival time must be after departure time." };
        }

        // Apply updates
        if (!string.IsNullOrWhiteSpace(request.Type))
            transport.Type = request.Type.Trim();
        if (request.DepartureLocation != null)
            transport.DepartureLocation = request.DepartureLocation.Trim();
        if (request.ArrivalLocation != null)
            transport.ArrivalLocation = request.ArrivalLocation.Trim();
        if (request.DepartureTime.HasValue)
            transport.DepartureTime = request.DepartureTime;
        if (request.ArrivalTime.HasValue)
            transport.ArrivalTime = request.ArrivalTime;
        if (request.Notes != null)
            transport.Notes = request.Notes.Trim();

        // Save to database
        await _transportRepository.UpdateAsync(transport, cancellationToken);

        // Map to response
        var response = new TransportResponse
        {
            Id = transport.Id,
            TripId = transport.TripId,
            Type = transport.Type,
            DepartureLocation = transport.DepartureLocation,
            ArrivalLocation = transport.ArrivalLocation,
            DepartureTime = transport.DepartureTime,
            ArrivalTime = transport.ArrivalTime,
            Notes = transport.Notes
        };

        return response;
    }
}