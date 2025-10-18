using Microsoft.EntityFrameworkCore;
using Trivare.Domain.Entities;
using Trivare.Domain.Interfaces;
using Trivare.Infrastructure.Data;

namespace Trivare.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Day entity operations
/// </summary>
public class DayRepository : IDayRepository
{
    private readonly ApplicationDbContext _context;

    public DayRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new day
    /// </summary>
    public async Task<Day> AddAsync(Day day, CancellationToken cancellationToken = default)
    {
        _context.Days.Add(day);
        await _context.SaveChangesAsync(cancellationToken);
        return day;
    }

    /// <summary>
    /// Gets a day by trip ID and date
    /// </summary>
    public async Task<Day?> GetByTripIdAndDateAsync(Guid tripId, DateOnly date, CancellationToken cancellationToken = default)
    {
        return await _context.Days
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.TripId == tripId && d.Date == date, cancellationToken);
    }

    /// <summary>
    /// Gets all days for a specific trip
    /// </summary>
    public async Task<IEnumerable<Day>> GetByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        return await _context.Days
            .AsNoTracking()
            .Where(d => d.TripId == tripId)
            .OrderBy(d => d.Date)
            .ToListAsync(cancellationToken);
    }
}