using Trivare.Domain.Entities;

namespace Trivare.Domain.Interfaces;

/// <summary>
/// Repository for Accommodation entity operations
/// </summary>
public interface IAccommodationRepository
{
    /// <summary>
    /// Creates a new accommodation
    /// </summary>
    /// <param name="accommodation">The accommodation entity to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created accommodation with generated ID</returns>
    Task<Accommodation> AddAsync(Accommodation accommodation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an accommodation by its ID
    /// </summary>
    /// <param name="accommodationId">The accommodation ID to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The accommodation entity or null if not found</returns>
    Task<Accommodation?> GetByIdAsync(Guid accommodationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an accommodation by trip ID
    /// </summary>
    /// <param name="tripId">The trip ID to retrieve accommodation for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The accommodation entity or null if not found</returns>
    Task<Accommodation?> GetByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing accommodation
    /// </summary>
    /// <param name="accommodation">The accommodation entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated accommodation entity</returns>
    Task<Accommodation> UpdateAsync(Accommodation accommodation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an accommodation
    /// </summary>
    /// <param name="accommodation">The accommodation entity to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(Accommodation accommodation, CancellationToken cancellationToken = default);
}