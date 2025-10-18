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
}