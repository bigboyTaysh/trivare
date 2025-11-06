using FluentAssertions;
using Trivare.Domain.Entities;
using Xunit;

namespace Trivare.Domain.Tests.Entities;

public class UserEntityTests
{
    #region Constructor and Property Initialization Tests

    [Fact]
    public void User_ShouldInitializeWithDefaultValues()
    {
        // Act
        var user = new User();

        // Assert
        user.Id.Should().BeEmpty();
        user.UserName.Should().BeNull();
        user.Email.Should().BeNull();
        user.PasswordHash.Should().BeNull();
        user.PasswordSalt.Should().BeNull();
        user.PasswordResetToken.Should().BeNull();
        user.PasswordResetTokenExpiry.Should().BeNull();
        user.RefreshToken.Should().BeNull();
        user.RefreshTokenExpiryTime.Should().BeNull();
        user.CreatedAt.Should().Be(default);
        user.UserRoles.Should().NotBeNull().And.BeEmpty();
        user.Trips.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public void User_ShouldAllowPropertyAssignment()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var passwordHash = new byte[256];
        var passwordSalt = new byte[128];
        var createdAt = DateTime.UtcNow;

        // Act
        var user = new User
        {
            Id = userId,
            UserName = "testuser",
            Email = "test@example.com",
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            PasswordResetToken = "reset_token_123",
            PasswordResetTokenExpiry = createdAt.AddHours(1),
            RefreshToken = "refresh_token_123",
            RefreshTokenExpiryTime = createdAt.AddDays(7),
            CreatedAt = createdAt
        };

        // Assert
        user.Id.Should().Be(userId);
        user.UserName.Should().Be("testuser");
        user.Email.Should().Be("test@example.com");
        user.PasswordHash.Should().BeEquivalentTo(passwordHash);
        user.PasswordSalt.Should().BeEquivalentTo(passwordSalt);
        user.PasswordResetToken.Should().Be("reset_token_123");
        user.PasswordResetTokenExpiry.Should().Be(createdAt.AddHours(1));
        user.RefreshToken.Should().Be("refresh_token_123");
        user.RefreshTokenExpiryTime.Should().Be(createdAt.AddDays(7));
        user.CreatedAt.Should().Be(createdAt);
    }

    #endregion

    #region Relationship Tests

    [Fact]
    public void User_ShouldSupportRoleRelationships()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = "test@example.com"
        };

        var role1 = new Role
        {
            Id = Guid.NewGuid(),
            Name = "User"
        };

        var role2 = new Role
        {
            Id = Guid.NewGuid(),
            Name = "Admin"
        };

        var userRole1 = new UserRole
        {
            UserId = user.Id,
            User = user,
            RoleId = role1.Id,
            Role = role1
        };

        var userRole2 = new UserRole
        {
            UserId = user.Id,
            User = user,
            RoleId = role2.Id,
            Role = role2
        };

        user.UserRoles.Add(userRole1);
        user.UserRoles.Add(userRole2);

        // Assert
        user.UserRoles.Should().HaveCount(2);
        user.UserRoles.Should().Contain(userRole1);
        user.UserRoles.Should().Contain(userRole2);
        userRole1.User.Should().Be(user);
        userRole2.User.Should().Be(user);
    }

    [Fact]
    public void User_ShouldSupportTripRelationships()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = "test@example.com"
        };

        var trip1 = new Trip
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Name = "Trip 1",
            StartDate = new DateOnly(2024, 7, 1),
            EndDate = new DateOnly(2024, 7, 5)
        };

        var trip2 = new Trip
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Name = "Trip 2",
            StartDate = new DateOnly(2024, 8, 1),
            EndDate = new DateOnly(2024, 8, 5)
        };

        user.Trips.Add(trip1);
        user.Trips.Add(trip2);

        // Assert
        user.Trips.Should().HaveCount(2);
        user.Trips.Should().Contain(trip1);
        user.Trips.Should().Contain(trip2);
        trip1.User.Should().Be(user);
        trip2.User.Should().Be(user);
        trip1.UserId.Should().Be(user.Id);
        trip2.UserId.Should().Be(user.Id);
    }

    #endregion

    #region Password Reset Token Tests

    [Fact]
    public void User_ShouldSupportPasswordResetTokenLifecycle()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = "test@example.com"
        };

        // Act - Set password reset token
        var token = "reset_token_123";
        var expiry = DateTime.UtcNow.AddHours(1);
        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = expiry;

        // Assert
        user.PasswordResetToken.Should().Be(token);
        user.PasswordResetTokenExpiry.Should().Be(expiry);

        // Act - Clear password reset token
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        // Assert
        user.PasswordResetToken.Should().BeNull();
        user.PasswordResetTokenExpiry.Should().BeNull();
    }

    [Fact]
    public void User_ShouldSupportExpiredPasswordResetTokens()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = "test@example.com",
            PasswordResetToken = "expired_token",
            PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(-1) // Expired
        };

        // Assert
        user.PasswordResetToken.Should().Be("expired_token");
        user.PasswordResetTokenExpiry.Should().BeBefore(DateTime.UtcNow);
    }

    #endregion

    #region Refresh Token Tests

    [Fact]
    public void User_ShouldSupportRefreshTokenLifecycle()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = "test@example.com"
        };

        // Act - Set refresh token
        var token = "refresh_token_123";
        var expiry = DateTime.UtcNow.AddDays(7);
        user.RefreshToken = token;
        user.RefreshTokenExpiryTime = expiry;

        // Assert
        user.RefreshToken.Should().Be(token);
        user.RefreshTokenExpiryTime.Should().Be(expiry);

        // Act - Clear refresh token (logout)
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        // Assert
        user.RefreshToken.Should().BeNull();
        user.RefreshTokenExpiryTime.Should().BeNull();
    }

    [Fact]
    public void User_ShouldSupportExpiredRefreshTokens()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "testuser",
            Email = "test@example.com",
            RefreshToken = "expired_refresh_token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1) // Expired
        };

        // Assert
        user.RefreshToken.Should().Be("expired_refresh_token");
        user.RefreshTokenExpiryTime.Should().BeBefore(DateTime.UtcNow);
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public void User_ShouldMaintainDataConsistency()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "originaluser",
            Email = "original@example.com",
            PasswordHash = new byte[256],
            PasswordSalt = new byte[128],
            CreatedAt = DateTime.UtcNow
        };

        // Act - Modify properties
        var newUserName = "updateduser";
        var newEmail = "updated@example.com";
        user.UserName = newUserName;
        user.Email = newEmail;

        // Assert
        user.UserName.Should().Be(newUserName);
        user.Email.Should().Be(newEmail);
        user.Id.Should().NotBeEmpty();
        user.CreatedAt.Should().NotBe(default);
        user.PasswordHash.Should().NotBeNull();
        user.PasswordSalt.Should().NotBeNull();
    }

    [Fact]
    public void User_ShouldHandleNullOptionalProperties()
    {
        // Arrange & Act
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "minimaluser",
            Email = "minimal@example.com",
            PasswordHash = new byte[256],
            PasswordSalt = new byte[128],
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        user.PasswordResetToken.Should().BeNull();
        user.PasswordResetTokenExpiry.Should().BeNull();
        user.RefreshToken.Should().BeNull();
        user.RefreshTokenExpiryTime.Should().BeNull();
    }

    #endregion

    #region Security Tests

    [Fact]
    public void User_ShouldStorePasswordComponentsSecurely()
    {
        // Arrange
        var passwordHash = new byte[256];
        var passwordSalt = new byte[128];

        // Fill with test data
        for (int i = 0; i < passwordHash.Length; i++) passwordHash[i] = (byte)(i % 256);
        for (int i = 0; i < passwordSalt.Length; i++) passwordSalt[i] = (byte)(i % 256);

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = "secureuser",
            Email = "secure@example.com",
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            CreatedAt = DateTime.UtcNow
        };

        // Assert
        user.PasswordHash.Should().BeEquivalentTo(passwordHash);
        user.PasswordSalt.Should().BeEquivalentTo(passwordSalt);
        user.PasswordHash.Length.Should().Be(256);
        user.PasswordSalt.Length.Should().Be(128);
    }

    #endregion
}
