using Trivare.Application.DTOs.Transport;
using Trivare.Application.DTOs.Accommodation;
using Trivare.Application.DTOs.Days;
using Trivare.Application.DTOs.Files;

namespace Trivare.Application.DTOs.Trips;

/// <summary>
/// Detailed trip data with optional nested relations
/// Derived from Trip entity with related entities based on include parameter
/// </summary>
public record TripDetailDto
{
    /// <summary>
    /// Trip identifier from Trip.Id
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Trip name from Trip.Name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Trip destination from Trip.Destination
    /// </summary>
    public string? Destination { get; init; }

    /// <summary>
    /// Trip start date from Trip.StartDate
    /// </summary>
    public required DateOnly StartDate { get; init; }

    /// <summary>
    /// Trip end date from Trip.EndDate
    /// </summary>
    public required DateOnly EndDate { get; init; }

    /// <summary>
    /// Additional notes from Trip.Notes
    /// </summary>
    public string? Notes { get; init; }

    /// <summary>
    /// Trip creation timestamp from Trip.CreatedAt
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// Transportation details from Trip.Transports (included when requested)
    /// </summary>
    public IEnumerable<TransportDto>? Transports { get; init; }

    /// <summary>
    /// Accommodation details from Trip.Accommodation (included when requested)
    /// </summary>
    public AccommodationDto? Accommodation { get; init; }

    /// <summary>
    /// Trip days from Trip.Days (included when requested)
    /// </summary>
    public IEnumerable<DayWithPlacesDto>? Days { get; init; }

    /// <summary>
    /// Trip files from Trip.Files (included when requested)
    /// </summary>
    public IEnumerable<FileDto>? Files { get; init; }
}
