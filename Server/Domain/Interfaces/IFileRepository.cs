using Trivare.Domain.Entities;

namespace Trivare.Domain.Interfaces;

/// <summary>
/// Repository for File entity operations
/// </summary>
public interface IFileRepository
{
    /// <summary>
    /// Adds a new file to the database
    /// </summary>
    /// <param name="file">The file entity to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The added file with generated ID</returns>
    Task<Trivare.Domain.Entities.File> AddAsync(Trivare.Domain.Entities.File file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a file by its ID
    /// </summary>
    /// <param name="id">The file ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The file or null if not found</returns>
    Task<Trivare.Domain.Entities.File?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the total number of files associated with a trip
    /// Includes files attached to trip, transports, accommodations, and days within the trip
    /// </summary>
    /// <param name="tripId">The trip ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The count of files for the trip</returns>
    Task<int> CountFilesByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all files associated with a trip
    /// Includes files attached to trip, transports, accommodations, and days within the trip
    /// </summary>
    /// <param name="tripId">The trip ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of files for the trip</returns>
    Task<IEnumerable<Trivare.Domain.Entities.File>> GetFilesByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default);
}