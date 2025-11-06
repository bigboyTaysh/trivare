using FluentAssertions;
using Trivare.Domain.Entities;
using Xunit;

namespace Trivare.Domain.Tests.Entities;

public class FileEntityTests
{
    #region Constructor and Property Initialization Tests

    [Fact]
    public void FileEntity_ShouldInitializeWithDefaultValues()
    {
        // Act
        var file = new Trivare.Domain.Entities.File();

        // Assert
        file.Id.Should().BeEmpty();
        file.FileName.Should().BeNull();
        file.FilePath.Should().BeNull();
        file.FileSize.Should().Be(0);
        file.FileType.Should().BeNull();
        file.CreatedAt.Should().Be(default);
        file.TripId.Should().BeNull();
        file.Trip.Should().BeNull();
        file.TransportId.Should().BeNull();
        file.Transport.Should().BeNull();
        file.AccommodationId.Should().BeNull();
        file.Accommodation.Should().BeNull();
        file.DayId.Should().BeNull();
        file.Day.Should().BeNull();
    }

    [Fact]
    public void FileEntity_ShouldAllowPropertyAssignment()
    {
        // Arrange
        var fileId = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        // Act
        var file = new Trivare.Domain.Entities.File
        {
            Id = fileId,
            FileName = "ticket.pdf",
            FilePath = "/files/trips/123/ticket.pdf",
            FileSize = 1024000,
            FileType = "application/pdf",
            CreatedAt = createdAt,
            TripId = Guid.NewGuid()
        };

        // Assert
        file.Id.Should().Be(fileId);
        file.FileName.Should().Be("ticket.pdf");
        file.FilePath.Should().Be("/files/trips/123/ticket.pdf");
        file.FileSize.Should().Be(1024000);
        file.FileType.Should().Be("application/pdf");
        file.CreatedAt.Should().Be(createdAt);
        file.TripId.Should().NotBeNull();
    }

    #endregion

    #region Relationship Tests

    [Fact]
    public void FileEntity_ShouldSupportTripRelationship()
    {
        // Arrange
        var trip = new Trip
        {
            Id = Guid.NewGuid(),
            Name = "Barcelona Trip",
            StartDate = new DateOnly(2024, 7, 1),
            EndDate = new DateOnly(2024, 7, 10)
        };

        var file = new Trivare.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            FileName = "itinerary.pdf",
            FilePath = "/files/itinerary.pdf",
            FileSize = 2048000,
            FileType = "application/pdf",
            CreatedAt = DateTime.UtcNow,
            TripId = trip.Id,
            Trip = trip
        };

        trip.Files.Add(file);

        // Assert
        file.Trip.Should().Be(trip);
        file.TripId.Should().Be(trip.Id);
        trip.Files.Should().Contain(file);
    }

    [Fact]
    public void FileEntity_ShouldSupportTransportRelationship()
    {
        // Arrange
        var transport = new Transport
        {
            Id = Guid.NewGuid(),
            TripId = Guid.NewGuid(),
            Type = "Flight",
            DepartureLocation = "London",
            ArrivalLocation = "Barcelona"
        };

        var file = new Trivare.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            FileName = "boarding_pass.pdf",
            FilePath = "/files/boarding_pass.pdf",
            FileSize = 512000,
            FileType = "application/pdf",
            CreatedAt = DateTime.UtcNow,
            TransportId = transport.Id,
            Transport = transport
        };

        transport.Files.Add(file);

        // Assert
        file.Transport.Should().Be(transport);
        file.TransportId.Should().Be(transport.Id);
        transport.Files.Should().Contain(file);
    }

    [Fact]
    public void FileEntity_ShouldSupportAccommodationRelationship()
    {
        // Arrange
        var accommodation = new Accommodation
        {
            Id = Guid.NewGuid(),
            TripId = Guid.NewGuid(),
            Name = "Hotel Arts",
            Address = "Barcelona, Spain"
        };

        var file = new Trivare.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            FileName = "booking_confirmation.pdf",
            FilePath = "/files/booking.pdf",
            FileSize = 768000,
            FileType = "application/pdf",
            CreatedAt = DateTime.UtcNow,
            AccommodationId = accommodation.Id,
            Accommodation = accommodation
        };

        accommodation.Files.Add(file);

        // Assert
        file.Accommodation.Should().Be(accommodation);
        file.AccommodationId.Should().Be(accommodation.Id);
        accommodation.Files.Should().Contain(file);
    }

    [Fact]
    public void FileEntity_ShouldSupportDayRelationship()
    {
        // Arrange
        var day = new Day
        {
            Id = Guid.NewGuid(),
            TripId = Guid.NewGuid(),
            Date = new DateOnly(2024, 7, 1),
            Notes = "Sightseeing day"
        };

        var file = new Trivare.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            FileName = "photos.zip",
            FilePath = "/files/day_photos.zip",
            FileSize = 52428800, // 50MB
            FileType = "application/zip",
            CreatedAt = DateTime.UtcNow,
            DayId = day.Id,
            Day = day
        };

        day.Files.Add(file);

        // Assert
        file.Day.Should().Be(day);
        file.DayId.Should().Be(day.Id);
        day.Files.Should().Contain(file);
    }

    #endregion

    #region FileEntity Size and Type Tests

    [Fact]
    public void FileEntity_ShouldSupportVariousFileSizes()
    {
        // Arrange & Act
        var smallFileEntity = new Trivare.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            FileName = "small.txt",
            FilePath = "/files/small.txt",
            FileSize = 1024, // 1KB
            FileType = "text/plain",
            CreatedAt = DateTime.UtcNow
        };

        var largeFileEntity = new Trivare.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            FileName = "large_video.mp4",
            FilePath = "/files/large.mp4",
            FileSize = 1073741824, // 1GB
            FileType = "video/mp4",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        smallFileEntity.FileSize.Should().Be(1024);
        largeFileEntity.FileSize.Should().Be(1073741824);
    }

    [Fact]
    public void FileEntity_ShouldSupportVariousFileTypes()
    {
        // Arrange
        var fileTypes = new[]
        {
            "application/pdf",
            "image/jpeg",
            "image/png",
            "video/mp4",
            "audio/mpeg",
            "text/plain",
            "application/zip",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
        };

        // Act & Assert
        foreach (var fileType in fileTypes)
        {
            var file = new Trivare.Domain.Entities.File
            {
                Id = Guid.NewGuid(),
                FileName = $"test.{GetExtensionFromMimeType(fileType)}",
                FilePath = $"/files/test.{GetExtensionFromMimeType(fileType)}",
                FileSize = 1024,
                FileType = fileType,
                CreatedAt = DateTime.UtcNow
            };

            file.FileType.Should().Be(fileType);
        }
    }

    #endregion

    #region Business Rule Tests

    [Fact]
    public void FileEntity_ShouldAllowOnlyOneParentRelationship()
    {
        // Arrange
        var file = new Trivare.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            FileName = "shared_document.pdf",
            FilePath = "/files/shared.pdf",
            FileSize = 1024000,
            FileType = "application/pdf",
            CreatedAt = DateTime.UtcNow
        };

        // Act - FileEntity can only be associated with one parent at a time
        // This is enforced by having only one non-null foreign key

        file.TripId = Guid.NewGuid();
        file.TransportId = null;
        file.AccommodationId = null;
        file.DayId = null;

        // Assert
        file.TripId.Should().NotBeNull();
        file.TransportId.Should().BeNull();
        file.AccommodationId.Should().BeNull();
        file.DayId.Should().BeNull();
    }

    [Fact]
    public void FileEntity_ShouldSupportOrphanedFiles()
    {
        // Arrange & Act
        var orphanedFileEntity = new Trivare.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            FileName = "temp_file.tmp",
            FilePath = "/files/temp.tmp",
            FileSize = 0,
            FileType = "application/octet-stream",
            CreatedAt = DateTime.UtcNow
            // All foreign keys remain null
        };

        // Assert
        orphanedFileEntity.TripId.Should().BeNull();
        orphanedFileEntity.TransportId.Should().BeNull();
        orphanedFileEntity.AccommodationId.Should().BeNull();
        orphanedFileEntity.DayId.Should().BeNull();
        orphanedFileEntity.Trip.Should().BeNull();
        orphanedFileEntity.Transport.Should().BeNull();
        orphanedFileEntity.Accommodation.Should().BeNull();
        orphanedFileEntity.Day.Should().BeNull();
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public void FileEntity_ShouldMaintainDataConsistency()
    {
        // Arrange
        var file = new Trivare.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            FileName = "original.pdf",
            FilePath = "/files/original.pdf",
            FileSize = 1024000,
            FileType = "application/pdf",
            CreatedAt = DateTime.UtcNow
        };

        // Act - Modify properties
        var newFileName = "updated.pdf";
        var newFileSize = 2048000L;
        file.FileName = newFileName;
        file.FileSize = newFileSize;

        // Assert
        file.FileName.Should().Be(newFileName);
        file.FileSize.Should().Be(newFileSize);
        file.Id.Should().NotBeEmpty();
        file.CreatedAt.Should().NotBe(default);
        file.FilePath.Should().NotBeNull();
        file.FileType.Should().NotBeNull();
    }

    [Fact]
    public void FileEntity_ShouldHandleLargeFileSizes()
    {
        // Arrange & Act
        var file = new Trivare.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            FileName = "huge_file.dat",
            FilePath = "/files/huge.dat",
            FileSize = long.MaxValue, // Maximum possible file size
            FileType = "application/octet-stream",
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        file.FileSize.Should().Be(long.MaxValue);
    }

    #endregion

    #region Helper Methods

    private static string GetExtensionFromMimeType(string mimeType)
    {
        return mimeType switch
        {
            "application/pdf" => "pdf",
            "image/jpeg" => "jpg",
            "image/png" => "png",
            "video/mp4" => "mp4",
            "audio/mpeg" => "mp3",
            "text/plain" => "txt",
            "application/zip" => "zip",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => "docx",
            _ => "bin"
        };
    }

    #endregion
}
