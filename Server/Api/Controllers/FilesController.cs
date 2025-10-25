using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Trivare.Api.Controllers.Utils;
using Trivare.Api.Extensions;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Files;
using Trivare.Application.Interfaces;

namespace Trivare.Api.Controllers;

/// <summary>
/// Controller for file upload operations
/// </summary>
[ApiController]
[Route("api")]
[Authorize]
[Produces("application/json")]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    /// <summary>
    /// Upload a file associated with a trip
    /// </summary>
    /// <param name="tripId">The unique identifier of the trip</param>
    /// <param name="file">The file to upload (PNG, JPEG, or PDF, max 5MB)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Details of the uploaded file including URLs</returns>
    /// <response code="201">File uploaded successfully</response>
    /// <response code="400">Invalid file type, size, or request data</response>
    /// <response code="403">User does not own the trip</response>
    /// <response code="404">Trip not found</response>
    /// <response code="409">Trip has reached the 10-file limit</response>
    /// <response code="500">Internal server error during upload</response>
    /// <response code="201">File uploaded successfully</response>
    /// <response code="400">Invalid file or request data</response>
    /// <response code="403">User does not own the trip</response>
    /// <response code="404">Trip not found</response>
    /// <response code="409">Trip has reached the file limit</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("trips/{tripId}/files")]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FileUploadResponse>> UploadTripFile(Guid tripId, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var fileData = new FileUploadData
        {
            Content = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length
        };
        var result = await _fileService.UploadTripFileAsync(tripId, fileData, userId, cancellationToken);

        return this.HandleResult(result, StatusCodes.Status201Created);
    }

    /// <summary>
    /// Upload a file associated with a transport
    /// </summary>
    /// <param name="transportId">The ID of the transport</param>
    /// <param name="file">The file to upload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File upload response</returns>
    /// <response code="201">File uploaded successfully</response>
    /// <response code="400">Invalid file or request data</response>
    /// <response code="403">User does not own the transport</response>
    /// <response code="404">Transport not found</response>
    /// <response code="409">Trip has reached the file limit</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("transports/{transportId}/files")]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FileUploadResponse>> UploadTransportFile(Guid transportId, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var fileData = new FileUploadData
        {
            Content = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length
        };
        var result = await _fileService.UploadTransportFileAsync(transportId, fileData, userId, cancellationToken);

        return this.HandleResult(result, StatusCodes.Status201Created);
    }

    /// <summary>
    /// Upload a file associated with an accommodation
    /// </summary>
    /// <param name="accommodationId">The ID of the accommodation</param>
    /// <param name="file">The file to upload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File upload response</returns>
    /// <response code="201">File uploaded successfully</response>
    /// <response code="400">Invalid file or request data</response>
    /// <response code="403">User does not own the accommodation</response>
    /// <response code="404">Accommodation not found</response>
    /// <response code="409">Trip has reached the file limit</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("accommodations/{accommodationId}/files")]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FileUploadResponse>> UploadAccommodationFile(Guid accommodationId, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var fileData = new FileUploadData
        {
            Content = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length
        };
        var result = await _fileService.UploadAccommodationFileAsync(accommodationId, fileData, userId, cancellationToken);

        return this.HandleResult(result, StatusCodes.Status201Created);
    }

    /// <summary>
    /// Upload a file associated with a day
    /// </summary>
    /// <param name="dayId">The ID of the day</param>
    /// <param name="file">The file to upload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>File upload response</returns>
    /// <response code="201">File uploaded successfully</response>
    /// <response code="400">Invalid file or request data</response>
    /// <response code="403">User does not own the day</response>
    /// <response code="404">Day not found</response>
    /// <response code="409">Trip has reached the file limit</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("days/{dayId}/files")]
    [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<FileUploadResponse>> UploadDayFile(Guid dayId, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var fileData = new FileUploadData
        {
            Content = file.OpenReadStream(),
            FileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length
        };
        var result = await _fileService.UploadDayFileAsync(dayId, fileData, userId, cancellationToken);

        return this.HandleResult(result, StatusCodes.Status201Created);
    }

    /// <summary>
    /// Get all files associated with a trip (with secure presigned URLs)
    /// </summary>
    /// <param name="tripId">The unique identifier of the trip</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of files with time-limited secure URLs</returns>
    /// <response code="200">Files retrieved successfully with presigned URLs</response>
    /// <response code="403">User does not own the trip</response>
    /// <response code="404">Trip not found</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("trips/{tripId}/files")]
    [ProducesResponseType(typeof(IEnumerable<FileUploadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<FileUploadResponse>>> GetTripFiles(Guid tripId, CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _fileService.GetTripFilesAsync(tripId, userId, cancellationToken);

        return this.HandleResult(result);
    }

    /// <summary>
    /// Delete a file from storage and database
    /// </summary>
    /// <param name="fileId">The unique identifier of the file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on success</returns>
    /// <response code="204">File deleted successfully</response>
    /// <response code="403">User does not own the file</response>
    /// <response code="404">File not found</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("files/{fileId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteFile(Guid fileId, CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _fileService.DeleteFileAsync(fileId, userId, cancellationToken);

        return this.HandleResult(result, StatusCodes.Status204NoContent);
    }
}