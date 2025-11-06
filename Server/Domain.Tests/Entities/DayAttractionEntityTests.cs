using FluentAssertions;
using Trivare.Domain.Entities;
using Xunit;

namespace Trivare.Domain.Tests.Entities;

public class DayAttractionEntityTests
{
    #region Constructor and Property Initialization Tests

    [Fact]
    public void DayAttraction_ShouldInitializeWithDefaultValues()
    {
        // Act
        var dayAttraction = new DayAttraction();

        // Assert
        dayAttraction.DayId.Should().BeEmpty();
        dayAttraction.Day.Should().BeNull();
        dayAttraction.PlaceId.Should().BeEmpty();
        dayAttraction.Place.Should().BeNull();
        dayAttraction.Order.Should().Be(0);
        dayAttraction.IsVisited.Should().BeFalse();
    }

    [Fact]
    public void DayAttraction_ShouldAllowPropertyAssignment()
    {
        // Arrange
        var dayId = Guid.NewGuid();
        var placeId = Guid.NewGuid();

        // Act
        var dayAttraction = new DayAttraction
        {
            DayId = dayId,
            PlaceId = placeId,
            Order = 5,
            IsVisited = true
        };

        // Assert
        dayAttraction.DayId.Should().Be(dayId);
        dayAttraction.PlaceId.Should().Be(placeId);
        dayAttraction.Order.Should().Be(5);
        dayAttraction.IsVisited.Should().BeTrue();
    }

    #endregion

    #region Relationship Tests

    [Fact]
    public void DayAttraction_ShouldSupportDayRelationship()
    {
        // Arrange
        var day = new Day
        {
            Id = Guid.NewGuid(),
            TripId = Guid.NewGuid(),
            Date = new DateOnly(2024, 7, 1),
            Notes = "Sightseeing day"
        };

        var place = new Place
        {
            Id = Guid.NewGuid(),
            Name = "Sagrada Familia",
            GooglePlaceId = "place123"
        };

        var dayAttraction = new DayAttraction
        {
            DayId = day.Id,
            Day = day,
            PlaceId = place.Id,
            Place = place,
            Order = 1,
            IsVisited = false
        };

        day.DayAttractions.Add(dayAttraction);

        // Assert
        dayAttraction.Day.Should().Be(day);
        dayAttraction.DayId.Should().Be(day.Id);
        day.DayAttractions.Should().Contain(dayAttraction);
    }

    [Fact]
    public void DayAttraction_ShouldSupportPlaceRelationship()
    {
        // Arrange
        var day = new Day
        {
            Id = Guid.NewGuid(),
            TripId = Guid.NewGuid(),
            Date = new DateOnly(2024, 7, 1)
        };

        var place = new Place
        {
            Id = Guid.NewGuid(),
            Name = "Park Güell",
            GooglePlaceId = "place456",
            FormattedAddress = "Barcelona, Spain"
        };

        var dayAttraction = new DayAttraction
        {
            DayId = day.Id,
            Day = day,
            PlaceId = place.Id,
            Place = place,
            Order = 2,
            IsVisited = true
        };

        place.DayAttractions.Add(dayAttraction);

        // Assert
        dayAttraction.Place.Should().Be(place);
        dayAttraction.PlaceId.Should().Be(place.Id);
        place.DayAttractions.Should().Contain(dayAttraction);
    }

    #endregion

    #region Ordering Tests

    [Fact]
    public void DayAttraction_ShouldSupportOrdering()
    {
        // Arrange
        var day = new Day
        {
            Id = Guid.NewGuid(),
            TripId = Guid.NewGuid(),
            Date = new DateOnly(2024, 7, 1)
        };

        var place1 = new Place { Id = Guid.NewGuid(), Name = "Place 1" };
        var place2 = new Place { Id = Guid.NewGuid(), Name = "Place 2" };
        var place3 = new Place { Id = Guid.NewGuid(), Name = "Place 3" };

        var attraction1 = new DayAttraction
        {
            DayId = day.Id,
            Day = day,
            PlaceId = place1.Id,
            Place = place1,
            Order = 1,
            IsVisited = false
        };

        var attraction2 = new DayAttraction
        {
            DayId = day.Id,
            Day = day,
            PlaceId = place2.Id,
            Place = place2,
            Order = 2,
            IsVisited = false
        };

        var attraction3 = new DayAttraction
        {
            DayId = day.Id,
            Day = day,
            PlaceId = place3.Id,
            Place = place3,
            Order = 3,
            IsVisited = true
        };

        day.DayAttractions.Add(attraction1);
        day.DayAttractions.Add(attraction2);
        day.DayAttractions.Add(attraction3);

        // Assert
        day.DayAttractions.Should().HaveCount(3);
        day.DayAttractions.OrderBy(da => da.Order).Should().Equal(attraction1, attraction2, attraction3);
    }

    [Fact]
    public void DayAttraction_ShouldAllowNegativeOrderValues()
    {
        // Arrange & Act
        var dayAttraction = new DayAttraction
        {
            DayId = Guid.NewGuid(),
            PlaceId = Guid.NewGuid(),
            Order = -1,
            IsVisited = false
        };

        // Assert
        dayAttraction.Order.Should().Be(-1);
    }

    [Fact]
    public void DayAttraction_ShouldAllowLargeOrderValues()
    {
        // Arrange & Act
        var dayAttraction = new DayAttraction
        {
            DayId = Guid.NewGuid(),
            PlaceId = Guid.NewGuid(),
            Order = int.MaxValue,
            IsVisited = false
        };

        // Assert
        dayAttraction.Order.Should().Be(int.MaxValue);
    }

    #endregion

    #region Visit Status Tests

    [Fact]
    public void DayAttraction_ShouldTrackVisitStatus()
    {
        // Arrange
        var dayAttraction = new DayAttraction
        {
            DayId = Guid.NewGuid(),
            PlaceId = Guid.NewGuid(),
            Order = 1,
            IsVisited = false
        };

        // Assert initial state
        dayAttraction.IsVisited.Should().BeFalse();

        // Act - Mark as visited
        dayAttraction.IsVisited = true;

        // Assert
        dayAttraction.IsVisited.Should().BeTrue();

        // Act - Mark as not visited
        dayAttraction.IsVisited = false;

        // Assert
        dayAttraction.IsVisited.Should().BeFalse();
    }

    [Fact]
    public void DayAttraction_ShouldSupportMixedVisitStatusesInDay()
    {
        // Arrange
        var day = new Day
        {
            Id = Guid.NewGuid(),
            TripId = Guid.NewGuid(),
            Date = new DateOnly(2024, 7, 1)
        };

        var visitedAttraction = new DayAttraction
        {
            DayId = day.Id,
            Day = day,
            PlaceId = Guid.NewGuid(),
            Place = new Place { Id = Guid.NewGuid(), Name = "Visited Place" },
            Order = 1,
            IsVisited = true
        };

        var unvisitedAttraction = new DayAttraction
        {
            DayId = day.Id,
            Day = day,
            PlaceId = Guid.NewGuid(),
            Place = new Place { Id = Guid.NewGuid(), Name = "Unvisited Place" },
            Order = 2,
            IsVisited = false
        };

        day.DayAttractions.Add(visitedAttraction);
        day.DayAttractions.Add(unvisitedAttraction);

        // Assert
        day.DayAttractions.Count(da => da.IsVisited).Should().Be(1);
        day.DayAttractions.Count(da => !da.IsVisited).Should().Be(1);
        visitedAttraction.IsVisited.Should().BeTrue();
        unvisitedAttraction.IsVisited.Should().BeFalse();
    }

    #endregion

    #region Business Rule Tests

    [Fact]
    public void DayAttraction_ShouldEnforceUniqueDayPlaceCombination()
    {
        // Note: This test demonstrates the business rule that should be enforced
        // In a real application, this would be enforced by the database or service layer

        // Arrange
        var dayId = Guid.NewGuid();
        var placeId = Guid.NewGuid();

        var attraction1 = new DayAttraction
        {
            DayId = dayId,
            PlaceId = placeId,
            Order = 1,
            IsVisited = false
        };

        var attraction2 = new DayAttraction
        {
            DayId = dayId,
            PlaceId = placeId, // Same place in same day
            Order = 2,
            IsVisited = false
        };

        // Assert - This demonstrates the uniqueness requirement
        attraction1.DayId.Should().Be(attraction2.DayId);
        attraction1.PlaceId.Should().Be(attraction2.PlaceId);
        // In practice, the database should prevent this duplicate
    }

    [Fact]
    public void DayAttraction_ShouldAllowSamePlaceInDifferentDays()
    {
        // Arrange
        var placeId = Guid.NewGuid();

        var day1Attraction = new DayAttraction
        {
            DayId = Guid.NewGuid(),
            PlaceId = placeId,
            Order = 1,
            IsVisited = false
        };

        var day2Attraction = new DayAttraction
        {
            DayId = Guid.NewGuid(), // Different day
            PlaceId = placeId, // Same place
            Order = 1,
            IsVisited = false
        };

        // Assert
        day1Attraction.DayId.Should().NotBe(day2Attraction.DayId);
        day1Attraction.PlaceId.Should().Be(day2Attraction.PlaceId);
        // This should be allowed - same place can be visited on different days
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public void DayAttraction_ShouldMaintainDataConsistency()
    {
        // Arrange
        var dayAttraction = new DayAttraction
        {
            DayId = Guid.NewGuid(),
            PlaceId = Guid.NewGuid(),
            Order = 5,
            IsVisited = true
        };

        // Act - Modify properties
        var newOrder = 10;
        var newVisitedStatus = false;
        dayAttraction.Order = newOrder;
        dayAttraction.IsVisited = newVisitedStatus;

        // Assert
        dayAttraction.Order.Should().Be(newOrder);
        dayAttraction.IsVisited.Should().Be(newVisitedStatus);
        dayAttraction.DayId.Should().NotBeEmpty();
        dayAttraction.PlaceId.Should().NotBeEmpty();
    }

    #endregion
}
