using Trivare.Domain.Entities;

namespace Trivare.Domain.Interfaces;

/// <summary>
/// Repository for DayAttraction entity operations
/// </summary>
public interface IDayAttractionRepository
{
    /// <summary>
    /// Creates a new day-attraction association
    /// </summary>
    /// <param name="dayAttraction">The day-attraction entity to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created day-attraction with generated ID</returns>
    Task<DayAttraction> AddAsync(DayAttraction dayAttraction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a day-attraction by day ID and place ID
    /// </summary>
    /// <param name="dayId">The day ID</param>
    /// <param name="placeId">The place ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The day-attraction entity or null if not found</returns>
    Task<DayAttraction?> GetByDayIdAndPlaceIdAsync(Guid dayId, Guid placeId, CancellationToken cancellationToken = default);
}