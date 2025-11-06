using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Trivare.Application.DTOs.Accommodation;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Days;
using Trivare.Application.DTOs.Places;
using Trivare.Application.DTOs.Transport;
using Trivare.Application.DTOs.Trips;
using Trivare.Application.Interfaces;
using Trivare.Application.Services;
using Trivare.Domain.Entities;
using Trivare.Domain.Interfaces;
using Xunit;

namespace Trivare.Application.Tests.Services;

public class TripServiceTests
{
    private readonly Mock<ITripRepository> _tripRepositoryMock;
    private readonly Mock<ITransportRepository> _transportRepositoryMock;
    private readonly Mock<IDayRepository> _dayRepositoryMock;
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;
    private readonly Mock<ILogger<TripService>> _loggerMock;
    private readonly TripService _tripService;

    public TripServiceTests()
    {
        _tripRepositoryMock = new Mock<ITripRepository>();
        _transportRepositoryMock = new Mock<ITransportRepository>();
        _dayRepositoryMock = new Mock<IDayRepository>();
        _auditLogRepositoryMock = new Mock<IAuditLogRepository>();
        _loggerMock = new Mock<ILogger<TripService>>();

        _tripService = new TripService(
            _tripRepositoryMock.Object,
            _transportRepositoryMock.Object,
            _dayRepositoryMock.Object,
            _auditLogRepositoryMock.Object,
            _loggerMock.Object);
    }

    #region Trip Creation Tests

    [Fact]
    public async Task CreateTripAsync_WithValidData_ShouldCreateTripSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateTripRequest
        {
            Name = "Summer Vacation",
            Destination = "Barcelona, Spain",
            StartDate = DateOnly.FromDateTime(new DateTime(2024, 7, 1)),
            EndDate = DateOnly.FromDateTime(new DateTime(2024, 7, 10)),
            Notes = "Family trip to Spain"
        };

        var expectedTripId = Guid.NewGuid();

        _tripRepositoryMock
            .Setup(r => r.CountByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3); // Below limit

        _tripRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Trip>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Trip trip, CancellationToken _) =>
            {
                trip.Id = expectedTripId;
                trip.CreatedAt = DateTime.UtcNow;
                return trip;
            });

        _auditLogRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _tripService.CreateTripAsync(request, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(expectedTripId);
        result.Value.UserId.Should().Be(userId);
        result.Value.Name.Should().Be(request.Name.Trim());
        result.Value.Destination.Should().Be(request.Destination.Trim());
        result.Value.StartDate.Should().Be(request.StartDate);
        result.Value.EndDate.Should().Be(request.EndDate);
        result.Value.Notes.Should().Be(request.Notes.Trim());

        _tripRepositoryMock.Verify(r => r.CountByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _tripRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Trip>(), It.IsAny<CancellationToken>()), Times.Once);
        _auditLogRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("", "Barcelona", "2024-07-01", "2024-07-10")]
    [InlineData("   ", "Barcelona", "2024-07-01", "2024-07-10")]
    [InlineData(null, "Barcelona", "2024-07-01", "2024-07-10")]
    public async Task CreateTripAsync_WithInvalidTripName_ShouldReturnInvalidTripDataError(string? name, string destination, string startDate, string endDate)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateTripRequest
        {
            Name = name!,
            Destination = destination,
            StartDate = DateOnly.Parse(startDate),
            EndDate = DateOnly.Parse(endDate)
        };

        // Act
        var result = await _tripService.CreateTripAsync(request, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Error.Should().Be(TripErrorCodes.InvalidTripData);
        result.Error.Message.Should().Be("Trip name cannot be empty or whitespace.");

        _tripRepositoryMock.Verify(r => r.CountByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _tripRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Trip>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateTripAsync_WithInvalidDateRange_ShouldReturnInvalidDateRangeError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateTripRequest
        {
            Name = "Invalid Trip",
            StartDate = DateOnly.FromDateTime(new DateTime(2024, 7, 10)),
            EndDate = DateOnly.FromDateTime(new DateTime(2024, 7, 1)) // End before start
        };

        // Act
        var result = await _tripService.CreateTripAsync(request, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Error.Should().Be(TripErrorCodes.TripInvalidDateRange);
        result.Error.Message.Should().Be("End date must be on or after start date.");

        _tripRepositoryMock.Verify(r => r.CountByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateTripAsync_WithTripLimitExceeded_ShouldReturnLimitExceededError()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateTripRequest
        {
            Name = "Too Many Trips",
            StartDate = DateOnly.FromDateTime(new DateTime(2024, 7, 1)),
            EndDate = DateOnly.FromDateTime(new DateTime(2024, 7, 10))
        };

        _tripRepositoryMock
            .Setup(r => r.CountByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(10); // At limit

        // Act
        var result = await _tripService.CreateTripAsync(request, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Error.Should().Be(TripErrorCodes.TripLimitExceeded);
        result.Error.Message.Should().Be("Maximum of 10 trips per user exceeded.");

        _tripRepositoryMock.Verify(r => r.CountByUserIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _tripRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Trip>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Trip Listing Tests

    [Fact]
    public async Task GetTripsAsync_WithValidRequest_ShouldReturnPaginatedTrips()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new TripListRequest
        {
            Page = 1,
            PageSize = 5,
            SortBy = "name",
            SortOrder = "asc",
            Search = "Barcelona"
        };

        var trips = new List<Trip>
        {
            new Trip
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Barcelona Trip",
                Destination = "Spain",
                StartDate = DateOnly.FromDateTime(new DateTime(2024, 7, 1)),
                EndDate = DateOnly.FromDateTime(new DateTime(2024, 7, 10)),
                CreatedAt = DateTime.UtcNow
            },
            new Trip
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Name = "Barcelona Extension",
                Destination = "Spain",
                StartDate = DateOnly.FromDateTime(new DateTime(2024, 8, 1)),
                EndDate = DateOnly.FromDateTime(new DateTime(2024, 8, 5)),
                CreatedAt = DateTime.UtcNow
            }
        };

        var totalItems = 2;

        _tripRepositoryMock
            .Setup(r => r.GetTripsPaginatedAsync(
                userId,
                request.Search,
                request.SortBy,
                request.SortOrder,
                request.Page,
                request.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((trips, totalItems));

        // Act
        var result = await _tripService.GetTripsAsync(request, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Data.Should().HaveCount(2);
        result.Value.Pagination.Should().NotBeNull();
        result.Value.Pagination!.Page.Should().Be(1);
        result.Value.Pagination.PageSize.Should().Be(5);
        result.Value.Pagination.TotalItems.Should().Be(2);
        result.Value.Pagination.TotalPages.Should().Be(1);

        _tripRepositoryMock.Verify(r => r.GetTripsPaginatedAsync(
            userId,
            request.Search,
            request.SortBy,
            request.SortOrder,
            request.Page,
            request.PageSize,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTripsAsync_WithNoResults_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new TripListRequest();

        _tripRepositoryMock
            .Setup(r => r.GetTripsPaginatedAsync(
                userId,
                request.Search,
                request.SortBy,
                request.SortOrder,
                request.Page,
                request.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Trip>(), 0));

        // Act
        var result = await _tripService.GetTripsAsync(request, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Data.Should().BeEmpty();
        result.Value.Pagination.Should().NotBeNull();
        result.Value.Pagination!.TotalItems.Should().Be(0);
        result.Value.Pagination.TotalPages.Should().Be(0);
    }

    #endregion

    #region Trip Update Tests

    [Fact]
    public async Task UpdateTripAsync_WithValidData_ShouldUpdateTripSuccessfully()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UpdateTripRequest
        {
            Name = "Updated Trip Name",
            Destination = "Updated Destination",
            StartDate = DateOnly.FromDateTime(new DateTime(2024, 8, 1)),
            EndDate = DateOnly.FromDateTime(new DateTime(2024, 8, 10)),
            Notes = "Updated notes"
        };

        var existingTrip = new Trip
        {
            Id = tripId,
            UserId = userId,
            Name = "Original Name",
            Destination = "Original Destination",
            StartDate = DateOnly.FromDateTime(new DateTime(2024, 7, 1)),
            EndDate = DateOnly.FromDateTime(new DateTime(2024, 7, 10)),
            Notes = "Original notes",
            CreatedAt = DateTime.UtcNow
        };

        _tripRepositoryMock
            .Setup(r => r.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTrip);

        _tripRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Trip>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Trip trip, CancellationToken _) => trip);

        // Act
        var result = await _tripService.UpdateTripAsync(tripId, request, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(tripId);
        result.Value.Name.Should().Be(request.Name.Trim());
        result.Value.Destination.Should().Be(request.Destination.Trim());
        result.Value.StartDate.Should().Be(request.StartDate);
        result.Value.EndDate.Should().Be(request.EndDate);
        result.Value.Notes.Should().Be(request.Notes.Trim());

        _tripRepositoryMock.Verify(r => r.GetByIdAsync(tripId, It.IsAny<CancellationToken>()), Times.Once);
        _tripRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Trip>(t =>
            t.Name == request.Name.Trim() &&
            t.Destination == request.Destination.Trim() &&
            t.StartDate == request.StartDate &&
            t.EndDate == request.EndDate &&
            t.Notes == request.Notes.Trim()), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateTripAsync_WithNonExistentTrip_ShouldReturnTripNotFoundError()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UpdateTripRequest
        {
            Name = "Updated Name"
        };

        _tripRepositoryMock
            .Setup(r => r.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Trip?)null);

        // Act
        var result = await _tripService.UpdateTripAsync(tripId, request, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Error.Should().Be(TripErrorCodes.TripNotFound);
        result.Error.Message.Should().Be("Trip not found.");

        _tripRepositoryMock.Verify(r => r.GetByIdAsync(tripId, It.IsAny<CancellationToken>()), Times.Once);
        _tripRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Trip>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTripAsync_WithTripOwnedByDifferentUser_ShouldReturnTripNotOwnedError()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();
        var request = new UpdateTripRequest
        {
            Name = "Updated Name"
        };

        var trip = new Trip
        {
            Id = tripId,
            UserId = differentUserId, // Different owner
            Name = "Original Name"
        };

        _tripRepositoryMock
            .Setup(r => r.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        // Act
        var result = await _tripService.UpdateTripAsync(tripId, request, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Error.Should().Be(TripErrorCodes.TripNotOwned);
        result.Error.Message.Should().Be("Access denied.");

        _tripRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Trip>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("", "Valid Destination")]
    [InlineData("   ", "Valid Destination")]
    public async Task UpdateTripAsync_WithInvalidName_ShouldReturnInvalidTripDataError(string name, string destination)
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UpdateTripRequest
        {
            Name = name,
            Destination = destination
        };

        var trip = new Trip
        {
            Id = tripId,
            UserId = userId,
            Name = "Original Name"
        };

        _tripRepositoryMock
            .Setup(r => r.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        // Act
        var result = await _tripService.UpdateTripAsync(tripId, request, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Error.Should().Be(TripErrorCodes.InvalidTripData);
        result.Error.Message.Should().Be("Trip name cannot be empty or whitespace.");

        _tripRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Trip>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateTripAsync_WithInvalidDateRange_ShouldReturnInvalidDateRangeError()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var request = new UpdateTripRequest
        {
            StartDate = DateOnly.FromDateTime(new DateTime(2024, 8, 10)),
            EndDate = DateOnly.FromDateTime(new DateTime(2024, 8, 1)) // Invalid range
        };

        var trip = new Trip
        {
            Id = tripId,
            UserId = userId,
            Name = "Original Name"
        };

        _tripRepositoryMock
            .Setup(r => r.GetByIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        // Act
        var result = await _tripService.UpdateTripAsync(tripId, request, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Error.Should().Be(TripErrorCodes.TripInvalidDateRange);
        result.Error.Message.Should().Be("End date must be on or after start date.");

        _tripRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Trip>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Get Trip by ID Tests

    [Fact]
    public async Task GetTripByIdAsync_WithValidTrip_ShouldReturnCompleteTripDetails()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var trip = new Trip
        {
            Id = tripId,
            UserId = userId,
            Name = "Barcelona Trip",
            Destination = "Spain",
            StartDate = DateOnly.FromDateTime(new DateTime(2024, 7, 1)),
            EndDate = DateOnly.FromDateTime(new DateTime(2024, 7, 10)),
            Notes = "Great trip",
            CreatedAt = DateTime.UtcNow,
            Accommodation = new Accommodation
            {
                Id = Guid.NewGuid(),
                Name = "Hotel Arts",
                Address = "Carrer Marina, Barcelona",
                CheckInDate = new DateTime(2024, 7, 1),
                CheckOutDate = new DateTime(2024, 7, 10),
                Notes = "Luxury hotel"
            }
        };

        var transports = new List<Transport>
        {
            new Transport
            {
                Id = Guid.NewGuid(),
                TripId = tripId,
                Type = "Flight",
                DepartureLocation = "London",
                ArrivalLocation = "Barcelona",
                DepartureTime = new DateTime(2024, 7, 1, 10, 0, 0),
                ArrivalTime = new DateTime(2024, 7, 1, 13, 0, 0),
                Notes = "Direct flight"
            }
        };

        var days = new List<Day>
        {
            new Day
            {
                Id = Guid.NewGuid(),
                TripId = tripId,
                Date = DateOnly.FromDateTime(new DateTime(2024, 7, 1)),
                Notes = "Arrival day",
                DayAttractions = new List<DayAttraction>
                {
                    new DayAttraction
                    {
                        DayId = Guid.NewGuid(),
                        PlaceId = Guid.NewGuid(),
                        Order = 1,
                        IsVisited = false,
                        Place = new Place
                        {
                            Id = Guid.NewGuid(),
                            GooglePlaceId = "place123",
                            Name = "Sagrada Familia",
                            FormattedAddress = "Barcelona, Spain",
                            Website = "https://sagradafamilia.org",
                            GoogleMapsLink = "https://maps.google.com/place123",
                            OpeningHoursText = "9 AM - 6 PM",
                            PhotoReference = "photo123",
                            IsManuallyAdded = false
                        }
                    }
                }
            }
        };

        _tripRepositoryMock
            .Setup(r => r.GetByIdWithAccommodationAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        _transportRepositoryMock
            .Setup(r => r.GetByTripIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(transports);

        _dayRepositoryMock
            .Setup(r => r.GetDaysWithPlacesByTripIdAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(days);

        // Act
        var result = await _tripService.GetTripByIdAsync(tripId, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(tripId);
        result.Value.Name.Should().Be(trip.Name);
        result.Value.Accommodation.Should().NotBeNull();
        result.Value.Accommodation!.Name.Should().Be("Hotel Arts");
        result.Value.Transports.Should().HaveCount(1);
        result.Value.Transports.First().Type.Should().Be("Flight");
        result.Value.Days.Should().HaveCount(1);
        result.Value.Days.First().Places.Should().HaveCount(1);
        result.Value.Days.First().Places.First().Place.Name.Should().Be("Sagrada Familia");

        _tripRepositoryMock.Verify(r => r.GetByIdWithAccommodationAsync(tripId, It.IsAny<CancellationToken>()), Times.Once);
        _transportRepositoryMock.Verify(r => r.GetByTripIdAsync(tripId, It.IsAny<CancellationToken>()), Times.Once);
        _dayRepositoryMock.Verify(r => r.GetDaysWithPlacesByTripIdAsync(tripId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTripByIdAsync_WithNonExistentTrip_ShouldReturnTripNotFoundError()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        _tripRepositoryMock
            .Setup(r => r.GetByIdWithAccommodationAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Trip?)null);

        // Act
        var result = await _tripService.GetTripByIdAsync(tripId, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Error.Should().Be(TripErrorCodes.TripNotFound);
        result.Error.Message.Should().Be("Trip not found.");

        _transportRepositoryMock.Verify(r => r.GetByTripIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _dayRepositoryMock.Verify(r => r.GetDaysWithPlacesByTripIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetTripByIdAsync_WithTripOwnedByDifferentUser_ShouldReturnTripAccessDeniedError()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var differentUserId = Guid.NewGuid();

        var trip = new Trip
        {
            Id = tripId,
            UserId = differentUserId, // Different owner
            Name = "Barcelona Trip"
        };

        _tripRepositoryMock
            .Setup(r => r.GetByIdWithAccommodationAsync(tripId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(trip);

        // Act
        var result = await _tripService.GetTripByIdAsync(tripId, userId);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Error.Should().Be(TripErrorCodes.TripAccessDenied);
        result.Error.Message.Should().Be("You do not have permission to access this trip.");

        _transportRepositoryMock.Verify(r => r.GetByTripIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _dayRepositoryMock.Verify(r => r.GetDaysWithPlacesByTripIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
