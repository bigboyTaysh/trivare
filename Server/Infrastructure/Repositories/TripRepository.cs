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
}