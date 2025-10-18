using Microsoft.EntityFrameworkCore;
using Trivare.Domain.Entities;
using Trivare.Domain.Interfaces;
using Trivare.Infrastructure.Data;

namespace Trivare.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Transport entity operations
/// </summary>
public class TransportRepository : ITransportRepository
{
    private readonly ApplicationDbContext _context;

    public TransportRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets a transport by its ID
    /// </summary>
    public async Task<Transport?> GetByIdAsync(Guid transportId, CancellationToken cancellationToken = default)
    {
        return await _context.Transport
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == transportId, cancellationToken);
    }

    /// <summary>
    /// Gets the transports for a specific trip
    /// </summary>
    public async Task<IEnumerable<Transport>> GetByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        return await _context.Transport
            .AsNoTracking()
            .Where(t => t.TripId == tripId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Creates a new transport
    /// </summary>
    public async Task<Transport> AddAsync(Transport transport, CancellationToken cancellationToken = default)
    {
        _context.Transport.Add(transport);
        await _context.SaveChangesAsync(cancellationToken);
        return transport;
    }

    /// <summary>
    /// Updates an existing transport
    /// </summary>
    public async Task<Transport> UpdateAsync(Transport transport, CancellationToken cancellationToken = default)
    {
        _context.Transport.Update(transport);
        await _context.SaveChangesAsync(cancellationToken);
        return transport;
    }
}