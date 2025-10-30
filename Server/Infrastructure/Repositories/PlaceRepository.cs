using Microsoft.EntityFrameworkCore;
using Trivare.Domain.Entities;
using Trivare.Domain.Interfaces;
using Trivare.Infrastructure.Data;

namespace Trivare.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Place entity operations
/// </summary>
public class PlaceRepository : IPlaceRepository
{
    private readonly ApplicationDbContext _context;

    public PlaceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new place
    /// </summary>
    public async Task<Place> AddAsync(Place place, CancellationToken cancellationToken = default)
    {
        _context.Places.Add(place);
        await _context.SaveChangesAsync(cancellationToken);
        return place;
    }

    /// <summary>
    /// Gets a place by ID
    /// </summary>
    public async Task<Place?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Places
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    /// <summary>
    /// Updates an existing place
    /// </summary>
    public async Task<Place> UpdateAsync(Place place, CancellationToken cancellationToken = default)
    {
        _context.Places.Update(place);
        await _context.SaveChangesAsync(cancellationToken);
        return place;
    }
}