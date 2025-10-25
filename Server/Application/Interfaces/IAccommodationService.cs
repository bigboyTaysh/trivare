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

    /// <summary>
    /// Updates accommodation for a trip owned by the authenticated user
    /// </summary>
    /// <param name="request">The accommodation update request with optional fields</param>
    /// <param name="tripId">The ID of the trip</param>
    /// <param name="userId">The ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated accommodation data or an error</returns>
    Task<Result<AccommodationDto>> UpdateAccommodationAsync(UpdateAccommodationRequest request, Guid tripId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes accommodation from a trip owned by the authenticated user
    /// </summary>
    /// <param name="tripId">The ID of the trip</param>
    /// <param name="userId">The ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success result or an error</returns>
    Task<Result<bool>> DeleteAccommodationAsync(Guid tripId, Guid userId, CancellationToken cancellationToken = default);
}