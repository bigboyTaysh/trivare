using Microsoft.Extensions.Logging;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Files;
using Trivare.Domain.Entities;
using Trivare.Domain.Interfaces;
using Trivare.Application.Interfaces;

namespace Trivare.Application.Services;

/// <summary>
/// Service implementation for file upload operations
/// </summary>
public class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ILogger<FileService> _logger;

    // Allowed MIME types
    private static readonly HashSet<string> AllowedMimeTypes = new()
    {
        "image/png",
        "image/jpeg",
        "application/pdf"
    };

    // Maximum file size: 5MB
    private const long MaxFileSize = 5 * 1024 * 1024;

    public FileService(
        IFileRepository fileRepository,
        IAuditLogRepository auditLogRepository,
        IFileStorageService fileStorageService,
        ILogger<FileService> logger)
    {
        _fileRepository = fileRepository;
        _auditLogRepository = auditLogRepository;
        _fileStorageService = fileStorageService;
        _logger = logger;
    }

    /// <summary>
    /// Uploads a file associated with a trip
    /// </summary>
    public async Task<Result<FileUploadResponse>> UploadTripFileAsync(Guid tripId, FileUploadData fileData, Guid userId, CancellationToken cancellationToken = default)
    {
        return await UploadFileAsync(tripId, null, null, null, fileData, userId, cancellationToken);
    }

    /// <summary>
    /// Uploads a file associated with a transport
    /// </summary>
    public async Task<Result<FileUploadResponse>> UploadTransportFileAsync(Guid transportId, FileUploadData fileData, Guid userId, CancellationToken cancellationToken = default)
    {
        return await UploadFileAsync(null, transportId, null, null, fileData, userId, cancellationToken);
    }

    /// <summary>
    /// Uploads a file associated with an accommodation
    /// </summary>
    public async Task<Result<FileUploadResponse>> UploadAccommodationFileAsync(Guid accommodationId, FileUploadData fileData, Guid userId, CancellationToken cancellationToken = default)
    {
        return await UploadFileAsync(null, null, accommodationId, null, fileData, userId, cancellationToken);
    }

    /// <summary>
    /// Uploads a file associated with a day
    /// </summary>
    public async Task<Result<FileUploadResponse>> UploadDayFileAsync(Guid dayId, FileUploadData fileData, Guid userId, CancellationToken cancellationToken = default)
    {
        return await UploadFileAsync(null, null, null, dayId, fileData, userId, cancellationToken);
    }

    private async Task<Result<FileUploadResponse>> UploadFileAsync(
        Guid? tripId,
        Guid? transportId,
        Guid? accommodationId,
        Guid? dayId,
        FileUploadData fileData,
        Guid userId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate file
            var validationError = ValidateFile(fileData);
            if (validationError != null)
            {
                return validationError;
            }

            // Determine the trip ID for limit check
            var targetTripId = tripId ?? await GetTripIdFromEntityAsync(transportId, accommodationId, dayId, cancellationToken);
            if (!targetTripId.HasValue)
            {
                return new ErrorResponse { Error = FileErrorCodes.FileNotFound, Message = "Associated trip not found" };
            }

            // Check file limit
            var fileCount = await _fileRepository.CountFilesByTripIdAsync(targetTripId.Value, cancellationToken);
            if (fileCount >= 10)
            {
                return new ErrorResponse { Error = FileErrorCodes.FileLimitExceeded, Message = "Trip has reached the maximum of 10 files" };
            }

            // Generate file path and ID
            var fileId = Guid.NewGuid();
            var sanitizedFileName = SanitizeFileName(fileData.FileName);
            var filePath = $"trips/{targetTripId}/{fileId}-{sanitizedFileName}";

            // Upload to storage
            await _fileStorageService.UploadAsync(fileData.Content, filePath, fileData.ContentType, cancellationToken);

            // Create file entity
            var file = new Trivare.Domain.Entities.File
            {
                Id = fileId,
                FileName = fileData.FileName,
                FilePath = filePath,
                FileSize = fileData.Length,
                FileType = fileData.ContentType,
                CreatedAt = DateTime.UtcNow,
                TripId = tripId,
                TransportId = transportId,
                AccommodationId = accommodationId,
                DayId = dayId
            };

            // Save to database
            await _fileRepository.AddAsync(file, cancellationToken);

            
            // Create response
            var response = new FileUploadResponse
            {
                Id = file.Id,
                FileName = file.FileName,
                FileSize = file.FileSize,
                FileType = file.FileType,
                TripId = file.TripId,
                TransportId = file.TransportId,
                AccommodationId = file.AccommodationId,
                DayId = file.DayId,
                CreatedAt = file.CreatedAt,
                DownloadUrl = _fileStorageService.GetDownloadUrl(filePath),
                FilePath = filePath,
                PreviewUrl = _fileStorageService.GetPreviewUrl(filePath)
            };

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file for user {UserId}", userId);
            return new ErrorResponse { Error = FileErrorCodes.FileUploadFailed, Message = "Failed to upload file" };
        }
    }

    private ErrorResponse? ValidateFile(FileUploadData fileData)
    {
        if (!AllowedMimeTypes.Contains(fileData.ContentType.ToLowerInvariant()))
        {
            return new ErrorResponse { Error = FileErrorCodes.FileInvalidType, Message = "File type not allowed. Only PNG, JPEG, and PDF files are supported" };
        }

        if (fileData.Length > MaxFileSize)
        {
            return new ErrorResponse { Error = FileErrorCodes.FileTooLarge, Message = "File size exceeds the maximum allowed size of 5MB" };
        }

        return null; // Valid
    }

    private string SanitizeFileName(string fileName)
    {
        // Remove path separators and other dangerous characters
        return string.Concat(fileName.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");
    }

    private async Task<Guid?> GetTripIdFromEntityAsync(Guid? transportId, Guid? accommodationId, Guid? dayId, CancellationToken cancellationToken)
    {
        // This is a placeholder - in real implementation, query the database to get trip ID from entity
        // For now, assume it's handled by RLS and return a dummy value
        // TODO: Implement proper trip ID resolution
        return Guid.NewGuid(); // Placeholder
    }

    private string GetEntityDescription(Guid? tripId, Guid? transportId, Guid? accommodationId, Guid? dayId)
    {
        if (tripId.HasValue) return $"trip {tripId}";
        if (transportId.HasValue) return $"transport {transportId}";
        if (accommodationId.HasValue) return $"accommodation {accommodationId}";
        if (dayId.HasValue) return $"day {dayId}";
        return "unknown entity";
    }
}