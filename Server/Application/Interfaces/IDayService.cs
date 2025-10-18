using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Days;

namespace Trivare.Application.Interfaces;

/// <summary>
/// Service interface for day-related operations
/// </summary>
public interface IDayService
{
    /// <summary>
    /// Creates a new day for a trip
    /// Validates trip ownership, date range, and ensures no duplicate dates
    /// </summary>
    /// <param name="request">The create day request</param>
    /// <param name="tripId">The trip identifier</param>
    /// <param name="userId">The user identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created day response or error</returns>
    Task<Result<CreateDayResponse>> CreateDayAsync(CreateDayRequest request, Guid tripId, Guid userId, CancellationToken cancellationToken = default);
}