using Microsoft.EntityFrameworkCore;
using Trivare.Domain.Entities;
using Trivare.Domain.Interfaces;
using Trivare.Infrastructure.Data;

namespace Trivare.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for DayAttraction entity operations
/// </summary>
public class DayAttractionRepository : IDayAttractionRepository
{
    private readonly ApplicationDbContext _context;

    public DayAttractionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new day-attraction association
    /// </summary>
    public async Task<DayAttraction> AddAsync(DayAttraction dayAttraction, CancellationToken cancellationToken = default)
    {
        _context.DayAttractions.Add(dayAttraction);
        await _context.SaveChangesAsync(cancellationToken);
        return dayAttraction;
    }

    /// <summary>
    /// Gets a day-attraction by day ID and place ID
    /// </summary>
    public async Task<DayAttraction?> GetByDayIdAndPlaceIdAsync(Guid dayId, Guid placeId, CancellationToken cancellationToken = default)
    {
        return await _context.DayAttractions
            .AsNoTracking()
            .FirstOrDefaultAsync(da => da.DayId == dayId && da.PlaceId == placeId, cancellationToken);
    }

    /// <summary>
    /// Updates an existing day-attraction association
    /// </summary>
    public async Task<DayAttraction> UpdateAsync(DayAttraction dayAttraction, CancellationToken cancellationToken = default)
    {
        _context.DayAttractions.Update(dayAttraction);
        await _context.SaveChangesAsync(cancellationToken);
        return dayAttraction;
    }

    /// <summary>
    /// Gets all day-attractions for a specific day including places
    /// </summary>
    public async Task<IEnumerable<DayAttraction>> GetByDayIdAsync(Guid dayId, CancellationToken cancellationToken = default)
    {
        return await _context.DayAttractions
            .AsNoTracking()
            .Include(da => da.Place)
            .Where(da => da.DayId == dayId)
            .OrderBy(da => da.Order)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes a day-attraction association
    /// </summary>
    public async Task DeleteAsync(DayAttraction dayAttraction, CancellationToken cancellationToken = default)
    {
        _context.DayAttractions.Remove(dayAttraction);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Checks if a user has access to a place through trip ownership
    /// User has access if they own at least one trip that contains this place
    /// </summary>
    public async Task<bool> UserHasAccessToPlaceAsync(Guid userId, Guid placeId, CancellationToken cancellationToken = default)
    {
        return await _context.DayAttractions
            .AnyAsync(da =>
                da.PlaceId == placeId &&
                da.Day != null &&
                da.Day.Trip != null &&
                da.Day.Trip.UserId == userId,
                cancellationToken);
    }
}