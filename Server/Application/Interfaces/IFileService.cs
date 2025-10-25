using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Files;

namespace Trivare.Application.Interfaces;

/// <summary>
/// Service for file upload operations
/// </summary>
public interface IFileService
{
    /// <summary>
    /// Uploads a file associated with a trip
    /// Validates file, checks business rules, stores in R2, saves metadata, and logs audit
    /// </summary>
    /// <param name="tripId">The ID of the trip to associate the file with</param>
    /// <param name="file">The uploaded file</param>
    /// <param name="userId">The ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The file upload response or an error</returns>
    Task<Result<FileUploadResponse>> UploadTripFileAsync(Guid tripId, FileUploadData file, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a file associated with a transport
    /// Validates file, checks business rules, stores in R2, saves metadata, and logs audit
    /// </summary>
    /// <param name="transportId">The ID of the transport to associate the file with</param>
    /// <param name="file">The uploaded file</param>
    /// <param name="userId">The ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The file upload response or an error</returns>
    Task<Result<FileUploadResponse>> UploadTransportFileAsync(Guid transportId, FileUploadData file, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a file associated with an accommodation
    /// Validates file, checks business rules, stores in R2, saves metadata, and logs audit
    /// </summary>
    /// <param name="accommodationId">The ID of the accommodation to associate the file with</param>
    /// <param name="file">The uploaded file</param>
    /// <param name="userId">The ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The file upload response or an error</returns>
    Task<Result<FileUploadResponse>> UploadAccommodationFileAsync(Guid accommodationId, FileUploadData file, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a file associated with a day
    /// Validates file, checks business rules, stores in R2, saves metadata, and logs audit
    /// </summary>
    /// <param name="dayId">The ID of the day to associate the file with</param>
    /// <param name="file">The uploaded file</param>
    /// <param name="userId">The ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The file upload response or an error</returns>
    Task<Result<FileUploadResponse>> UploadDayFileAsync(Guid dayId, FileUploadData file, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all files for a trip with secure presigned URLs
    /// </summary>
    /// <param name="tripId">The ID of the trip</param>
    /// <param name="userId">The ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of files with presigned URLs</returns>
    Task<Result<IEnumerable<FileUploadResponse>>> GetTripFilesAsync(Guid tripId, Guid userId, CancellationToken cancellationToken = default);
}