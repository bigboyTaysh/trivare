using Trivare.Application.DTOs.Accommodation;
using Trivare.Application.DTOs.Common;

namespace Trivare.Application.Interfaces;

/// <summary>
/// Service for accommodation-related operations
/// </summary>
public interface IAccommodationService
{
    /// <summary>
    /// Adds accommodation to a trip for the authenticated user
    /// </summary>
    /// <param name="request">The accommodation creation request</param>
    /// <param name="tripId">The ID of the trip</param>
    /// <param name="userId">The ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created accommodation data or an error</returns>
    Task<Result<AccommodationDto>> AddAccommodationAsync(CreateAccommodationRequest request, Guid tripId, Guid userId, CancellationToken cancellationToken = default);
}