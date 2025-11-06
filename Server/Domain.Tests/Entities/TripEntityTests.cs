using FluentAssertions;
using Trivare.Domain.Entities;
using Xunit;

namespace Trivare.Domain.Tests.Entities;

public class TripEntityTests
{
    #region Constructor and Property Initialization Tests

    [Fact]
    public void Trip_ShouldInitializeWithDefaultValues()
    {
        // Act
        var trip = new Trip();

        // Assert
        trip.Id.Should().BeEmpty();
        trip.UserId.Should().BeEmpty();
        trip.User.Should().BeNull();
        trip.Name.Should().BeNull();
        trip.Destination.Should().BeNull();
        trip.StartDate.Should().Be(default);
        trip.EndDate.Should().Be(default);
        trip.Notes.Should().BeNull();
        trip.CreatedAt.Should().Be(default);
        trip.Transports.Should().NotBeNull().And.BeEmpty();
        trip.Accommodation.Should().BeNull();
        trip.Days.Should().NotBeNull().And.BeEmpty();
        trip.Files.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void Trip_ShouldAllowPropertyAssignment()
    {
        // Arrange
        var tripId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var startDate = new DateOnly(2024, 7, 1);
        var endDate = new DateOnly(2024, 7, 10);
        var createdAt = DateTime.UtcNow;

        // Act
        var trip = new Trip
        {
            Id = tripId,
            UserId = userId,
            Name = "Summer Vacation",
            Destination = "Barcelona, Spain",
            StartDate = startDate,
            EndDate = endDate,
            Notes = "Family trip",
            CreatedAt = createdAt
        };

        // Assert
        trip.Id.Should().Be(tripId);
        trip.UserId.Should().Be(userId);
        trip.Name.Should().Be("Summer Vacation");
        trip.Destination.Should().Be("Barcelona, Spain");
        trip.StartDate.Should().Be(startDate);
        trip.EndDate.Should().Be(endDate);
        trip.Notes.Should().Be("Family trip");
        trip.CreatedAt.Should().Be(createdAt);
    }

    #endregion

    #region Relationship Tests

    [Fact]
    public void Trip_ShouldSupportUserRelationship()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = "test@example.com"
        };

        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Name = "Test Trip"
        };

        user.Trips.Add(trip);

        // Assert
        trip.User.Should().Be(user);
        trip.UserId.Should().Be(user.Id);
        user.Trips.Should().Contain(trip);
    }

    [Fact]
    public void Trip_ShouldSupportTransportRelationships()
    {
        // Arrange
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Test Trip"
        };

        var transport1 = new Transport
        {
            Id = Guid.NewGuid(),
            TripId = trip.Id,
            Trip = trip,
            Type = "Flight",
            DepartureLocation = "London",
            ArrivalLocation = "Barcelona"
        };

        var transport2 = new Transport
        {
            Id = Guid.NewGuid(),
            TripId = trip.Id,
            Trip = trip,
            Type = "Train",
            DepartureLocation = "Barcelona",
            ArrivalLocation = "Madrid"
        };

        trip.Transports.Add(transport1);
        trip.Transports.Add(transport2);

        // Assert
        trip.Transports.Should().HaveCount(2);
        trip.Transports.Should().Contain(transport1);
        trip.Transports.Should().Contain(transport2);
        transport1.Trip.Should().Be(trip);
        transport2.Trip.Should().Be(trip);
    }

    [Fact]
    public void Trip_ShouldSupportAccommodationRelationship()
    {
        // Arrange
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Test Trip"
        };

        var accommodation = new Accommodation
        {
            Id = Guid.NewGuid(),
            TripId = trip.Id,
            Trip = trip,
            Name = "Hotel Arts",
            Address = "Barcelona, Spain"
        };

        trip.Accommodation = accommodation;

        // Assert
        trip.Accommodation.Should().Be(accommodation);
        accommodation.Trip.Should().Be(trip);
        accommodation.TripId.Should().Be(trip.Id);
    }

    [Fact]
    public void Trip_ShouldSupportDayRelationships()
    {
        // Arrange
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Test Trip"
        };

        var day1 = new Day
        {
            Id = Guid.NewGuid(),
            TripId = trip.Id,
            Trip = trip,
            Date = new DateOnly(2024, 7, 1),
            Notes = "Arrival day"
        };

        var day2 = new Day
        {
            Id = Guid.NewGuid(),
            TripId = trip.Id,
            Trip = trip,
            Date = new DateOnly(2024, 7, 2),
            Notes = "Sightseeing day"
        };

        trip.Days.Add(day1);
        trip.Days.Add(day2);

        // Assert
        trip.Days.Should().HaveCount(2);
        trip.Days.Should().Contain(day1);
        trip.Days.Should().Contain(day2);
        day1.Trip.Should().Be(trip);
        day2.Trip.Should().Be(trip);
    }

    [Fact]
    public void Trip_ShouldSupportFileRelationships()
    {
        // Arrange
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Test Trip"
        };

        var file1 = new Trivare.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            TripId = trip.Id,
            Trip = trip,
            FileName = "ticket.pdf",
            FilePath = "/files/ticket.pdf",
            FileSize = 1024,
            FileType = "application/pdf"
        };

        var file2 = new Trivare.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            TripId = trip.Id,
            Trip = trip,
            FileName = "photo.jpg",
            FilePath = "/files/photo.jpg",
            FileSize = 2048,
            FileType = "image/jpeg"
        };

        trip.Files.Add(file1);
        trip.Files.Add(file2);

        // Assert
        trip.Files.Should().HaveCount(2);
        trip.Files.Should().Contain(file1);
        trip.Files.Should().Contain(file2);
        file1.Trip.Should().Be(trip);
        file2.Trip.Should().Be(trip);
    }

    #endregion

    #region Business Rule Tests

    [Fact]
    public void Trip_ShouldAllowValidDateRange()
    {
        // Arrange & Act
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Valid Trip",
            StartDate = new DateOnly(2024, 7, 1),
            EndDate = new DateOnly(2024, 7, 10)
        };

        // Assert
        trip.StartDate.Should().Be(new DateOnly(2024, 7, 1));
        trip.EndDate.Should().Be(new DateOnly(2024, 7, 10));
        trip.EndDate.Should().BeOnOrAfter(trip.StartDate);
    }

    [Fact]
    public void Trip_ShouldAllowSameDayTrips()
    {
        // Arrange & Act
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Day Trip",
            StartDate = new DateOnly(2024, 7, 1),
            EndDate = new DateOnly(2024, 7, 1)
        };

        // Assert
        trip.StartDate.Should().Be(trip.EndDate);
    }

    [Fact]
    public void Trip_ShouldAllowFutureDates()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(1));

        // Act
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Future Trip",
            StartDate = futureDate,
            EndDate = futureDate.AddDays(7)
        };

        // Assert
        trip.StartDate.Should().BeOnOrAfter(DateOnly.FromDateTime(DateTime.UtcNow));
    }

    [Fact]
    public void Trip_ShouldAllowPastDates()
    {
        // Arrange
        var pastDate = new DateOnly(2020, 1, 1);

        // Act
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Past Trip",
            StartDate = pastDate,
            EndDate = pastDate.AddDays(7)
        };

        // Assert
        trip.StartDate.Should().Be(pastDate);
        trip.EndDate.Should().Be(pastDate.AddDays(7));
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public void Trip_ShouldMaintainDataConsistency()
    {
        // Arrange
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Consistency Test Trip",
            StartDate = new DateOnly(2024, 7, 1),
            EndDate = new DateOnly(2024, 7, 5),
            CreatedAt = DateTime.UtcNow
        };

        // Act - Modify properties
        var newName = "Updated Trip Name";
        var newDestination = "Updated Destination";
        trip.Name = newName;
        trip.Destination = newDestination;

        // Assert
        trip.Name.Should().Be(newName);
        trip.Destination.Should().Be(newDestination);
        trip.Id.Should().NotBeEmpty();
        trip.UserId.Should().NotBeEmpty();
        trip.CreatedAt.Should().NotBe(default);
    }

    [Fact]
    public void Trip_ShouldHandleNullOptionalProperties()
    {
        // Arrange & Act
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Minimal Trip",
            StartDate = new DateOnly(2024, 7, 1),
            EndDate = new DateOnly(2024, 7, 1)
        };

        // Assert
        trip.Destination.Should().BeNull();
        trip.Notes.Should().BeNull();
        trip.Accommodation.Should().BeNull();
    }

    #endregion
}
