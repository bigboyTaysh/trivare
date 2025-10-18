using Microsoft.EntityFrameworkCore;
using Trivare.Domain.Entities;
using Trivare.Domain.Interfaces;
using Trivare.Infrastructure.Data;

namespace Trivare.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Trip entity operations
/// </summary>
public class TripRepository : ITripRepository
{
    private readonly ApplicationDbContext _context;

    public TripRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new trip
    /// </summary>
    public async Task<Trip> AddAsync(Trip trip, CancellationToken cancellationToken = default)
    {
        _context.Trips.Add(trip);
        await _context.SaveChangesAsync(cancellationToken);
        return trip;
    }

    /// <summary>
    /// Gets all trips for a specific user
    /// </summary>
    public async Task<ICollection<Trip>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Trips
            .Where(t => t.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Counts the number of trips for a specific user
    /// Uses AsNoTracking for optimal read-only performance
    /// </summary>
    public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Trips
            .AsNoTracking()
            .CountAsync(t => t.UserId == userId, cancellationToken);
    }

    /// <summary>
    /// Gets a trip by its ID
    /// </summary>
    public async Task<Trip?> GetByIdAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        return await _context.Trips
            .FirstOrDefaultAsync(t => t.Id == tripId, cancellationToken);
    }

    /// <summary>
    /// Updates an existing trip
    /// </summary>
    public async Task<Trip> UpdateAsync(Trip trip, CancellationToken cancellationToken = default)
    {
        _context.Trips.Update(trip);
        await _context.SaveChangesAsync(cancellationToken);
        return trip;
    }

    /// <summary>
    /// Gets a paginated list of trips with filtering and sorting applied
    /// </summary>
    public async Task<(IEnumerable<Trip> Trips, int TotalCount)> GetTripsPaginatedAsync(Guid userId, string? search, string sortBy, string sortOrder, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = _context.Trips
            .AsNoTracking()
            .Where(t => t.UserId == userId);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchTerm = search.Trim();
            query = query.Where(t => 
                t.Name.Contains(searchTerm) || 
                (t.Destination != null && t.Destination.Contains(searchTerm)) ||
                (t.Notes != null && t.Notes.Contains(searchTerm)) ||
                t.StartDate.ToString().Contains(searchTerm) ||
                t.EndDate.ToString().Contains(searchTerm));
        }

        // Apply sorting
        query = sortBy switch
        {
            "name" => sortOrder == "asc" ? query.OrderBy(t => t.Name) : query.OrderByDescending(t => t.Name),
            "startDate" => sortOrder == "asc" ? query.OrderBy(t => t.StartDate) : query.OrderByDescending(t => t.StartDate),
            _ => sortOrder == "asc" ? query.OrderBy(t => t.CreatedAt) : query.OrderByDescending(t => t.CreatedAt)
        };

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var trips = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (trips, totalCount);
    }

    /// <summary>
    /// Gets the transport for a specific trip
    /// </summary>
    public async Task<Transport?> GetTransportByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        return await _context.Transport
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TripId == tripId, cancellationToken);
    }

    /// <summary>
    /// Creates a new transport
    /// </summary>
    public async Task<Transport> AddTransportAsync(Transport transport, CancellationToken cancellationToken = default)
    {
        _context.Transport.Add(transport);
        await _context.SaveChangesAsync(cancellationToken);
        return transport;
    }
}