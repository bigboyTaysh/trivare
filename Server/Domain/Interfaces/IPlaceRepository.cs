using Trivare.Domain.Entities;

namespace Trivare.Domain.Interfaces;

/// <summary>
/// Repository for Place entity operations
/// </summary>
public interface IPlaceRepository
{
    /// <summary>
    /// Creates a new place
    /// </summary>
    /// <param name="place">The place entity to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created place with generated ID</returns>
    Task<Place> AddAsync(Place place, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a place by ID
    /// </summary>
    /// <param name="id">The place ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The place entity or null if not found</returns>
    Task<Place?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing place
    /// </summary>
    /// <param name="place">The place entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated place entity</returns>
    Task<Place> UpdateAsync(Place place, CancellationToken cancellationToken = default);
}