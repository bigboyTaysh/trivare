using Microsoft.EntityFrameworkCore;
using Trivare.Domain.Entities;
using Trivare.Domain.Interfaces;
using Trivare.Infrastructure.Data;

namespace Trivare.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for File entity operations
/// </summary>
public class FileRepository : IFileRepository
{
    private readonly ApplicationDbContext _context;

    public FileRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adds a new file to the database
    /// </summary>
    public async Task<Trivare.Domain.Entities.File> AddAsync(Trivare.Domain.Entities.File file, CancellationToken cancellationToken = default)
    {
        _context.Files.Add(file);
        await _context.SaveChangesAsync(cancellationToken);
        return file;
    }

    /// <summary>
    /// Gets a file by its ID
    /// </summary>
    public async Task<Trivare.Domain.Entities.File?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Files
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    /// <summary>
    /// Counts the total number of files associated with a trip
    /// Includes files attached to trip, transports, accommodations, and days within the trip
    /// </summary>
    public async Task<int> CountFilesByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        return await _context.Files
            .CountAsync(f =>
                f.TripId == tripId ||
                (f.TransportId.HasValue && _context.Transport.Any(t => t.Id == f.TransportId && t.TripId == tripId)) ||
                (f.AccommodationId.HasValue && _context.Accommodations.Any(a => a.Id == f.AccommodationId && a.TripId == tripId)) ||
                (f.DayId.HasValue && _context.Days.Any(d => d.Id == f.DayId && d.TripId == tripId)),
                cancellationToken);
    }

    /// <summary>
    /// Gets all files associated with a trip
    /// Includes files attached to trip, transports, accommodations, and days within the trip
    /// </summary>
    public async Task<IEnumerable<Trivare.Domain.Entities.File>> GetFilesByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        return await _context.Files
            .Where(f =>
                f.TripId == tripId ||
                (f.TransportId.HasValue && _context.Transport.Any(t => t.Id == f.TransportId && t.TripId == tripId)) ||
                (f.AccommodationId.HasValue && _context.Accommodations.Any(a => a.Id == f.AccommodationId && a.TripId == tripId)) ||
                (f.DayId.HasValue && _context.Days.Any(d => d.Id == f.DayId && d.TripId == tripId)))
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all files directly associated with a trip (trip-level files only)
    /// </summary>
    public async Task<IEnumerable<Trivare.Domain.Entities.File>> GetTripLevelFilesByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default)
    {
        return await _context.Files
            .Where(f => f.TripId == tripId && f.AccommodationId == null && f.TransportId == null && f.DayId == null)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Gets all files associated with an accommodation
    /// </summary>
    public async Task<IEnumerable<Trivare.Domain.Entities.File>> GetFilesByAccommodationIdAsync(Guid accommodationId, CancellationToken cancellationToken = default)
    {
        return await _context.Files
            .Where(f => f.AccommodationId == accommodationId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes a file from the database
    /// </summary>
    public async Task DeleteAsync(Trivare.Domain.Entities.File file, CancellationToken cancellationToken = default)
    {
        _context.Files.Remove(file);
        await _context.SaveChangesAsync(cancellationToken);
    }
}