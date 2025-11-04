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

    /// <summary>
    /// Updates an existing day-attraction association
    /// </summary>
    /// <param name="dayAttraction">The day-attraction entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated day-attraction entity</returns>
    Task<DayAttraction> UpdateAsync(DayAttraction dayAttraction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all day-attractions for a specific day
    /// </summary>
    /// <param name="dayId">The day ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of day-attraction entities for the day</returns>
    Task<IEnumerable<DayAttraction>> GetByDayIdAsync(Guid dayId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a day-attraction association
    /// </summary>
    /// <param name="dayAttraction">The day-attraction entity to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(DayAttraction dayAttraction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has access to a place through trip ownership
    /// User has access if they own at least one trip that contains this place
    /// </summary>
    /// <param name="userId">The user ID</param>
    /// <param name="placeId">The place ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if user has access, false otherwise</returns>
    Task<bool> UserHasAccessToPlaceAsync(Guid userId, Guid placeId, CancellationToken cancellationToken = default);
}