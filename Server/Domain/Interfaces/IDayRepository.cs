using Trivare.Domain.Entities;

namespace Trivare.Domain.Interfaces;

/// <summary>
/// Repository for Day entity operations
/// </summary>
public interface IDayRepository
{
    /// <summary>
    /// Creates a new day
    /// </summary>
    /// <param name="day">The day entity to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created day with generated ID</returns>
    Task<Day> AddAsync(Day day, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a day by trip ID and date
    /// </summary>
    /// <param name="tripId">The trip ID</param>
    /// <param name="date">The date</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The day entity or null if not found</returns>
    Task<Day?> GetByTripIdAndDateAsync(Guid tripId, DateOnly date, CancellationToken cancellationToken = default);
}