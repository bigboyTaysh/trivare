using Microsoft.EntityFrameworkCore;
using Trivare.Domain.Entities;
using Trivare.Domain.Interfaces;
using Trivare.Infrastructure.Data;

namespace Trivare.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Accommodation entity operations
/// </summary>
public class AccommodationRepository : IAccommodationRepository
{
    private readonly ApplicationDbContext _context;

    public AccommodationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new accommodation
    /// </summary>
    public async Task<Accommodation> AddAsync(Accommodation accommodation, CancellationToken cancellationToken = default)
    {
        _context.Accommodations.Add(accommodation);
        await _context.SaveChangesAsync(cancellationToken);
        return accommodation;
    }

    /// <summary>
    /// Gets an accommodation by its ID
    /// </summary>
    public async Task<Accommodation?> GetByIdAsync(Guid accommodationId, CancellationToken cancellationToken = default)
    {
        return await _context.Accommodations
            .FirstOrDefaultAsync(a => a.Id == accommodationId, cancellationToken);
    }

    /// <summary>
    /// Gets an accommodation by trip ID
    /// </summary>
    public async Task<Accommodation?> GetByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        return await _context.Accommodations
            .FirstOrDefaultAsync(a => a.TripId == tripId, cancellationToken);
    }

    /// <summary>
    /// Updates an existing accommodation
    /// </summary>
    public async Task<Accommodation> UpdateAsync(Accommodation accommodation, CancellationToken cancellationToken = default)
    {
        _context.Accommodations.Update(accommodation);
        await _context.SaveChangesAsync(cancellationToken);
        return accommodation;
    }
}