using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Trivare.Application.DTOs.Auth;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Users;
using Trivare.Application.Interfaces;
using Trivare.Application.Services;
using Trivare.Domain.Entities;
using Trivare.Domain.Interfaces;
using Xunit;

namespace Trivare.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepositoryMock;
    private readonly Mock<IRoleRepository> _roleRepositoryMock;
    private readonly Mock<IAuditLogRepository> _auditLogRepositoryMock;
    private readonly Mock<IPasswordHashingService> _passwordHashingServiceMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<ILogger<AuthService>> _loggerMock;
    private readonly Mock<IEmailService> _emailServiceMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _auditLogRepositoryMock = new Mock<IAuditLogRepository>();
        _passwordHashingServiceMock = new Mock<IPasswordHashingService>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _loggerMock = new Mock<ILogger<AuthService>>();
        _emailServiceMock = new Mock<IEmailService>();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _auditLogRepositoryMock.Object,
            _passwordHashingServiceMock.Object,
            _jwtTokenServiceMock.Object,
            _loggerMock.Object,
            _emailServiceMock.Object);
    }

    #region Registration Tests

    [Fact]
    public async Task RegisterAsync_WithValidData_ShouldCreateUserSuccessfully()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            UserName = "testuser",
            Password = "SecurePass123!"
        };

        var defaultRole = new Role { Id = Guid.NewGuid(), Name = "User" };
        var expectedUserId = Guid.NewGuid();

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _roleRepositoryMock
            .Setup(r => r.GetByNameAsync("User", It.IsAny<CancellationToken>()))
            .ReturnsAsync(defaultRole);

        _passwordHashingServiceMock
            .Setup(s => s.HashPassword(request.Password))
            .Returns((new byte[256], new byte[128]));

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) =>
            {
                user.Id = expectedUserId;
                user.CreatedAt = DateTime.UtcNow;
                return user;
            });

        _auditLogRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Id.Should().Be(expectedUserId);
        result.Value.Email.Should().Be("test@example.com");
        result.Value.UserName.Should().Be("testuser");

        _userRepositoryMock.Verify(r => r.EmailExistsAsync("test@example.com", It.IsAny<CancellationToken>()), Times.Once);
        _roleRepositoryMock.Verify(r => r.GetByNameAsync("User", It.IsAny<CancellationToken>()), Times.Once);
        _passwordHashingServiceMock.Verify(s => s.HashPassword(request.Password), Times.Once);
        _userRepositoryMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _auditLogRepositoryMock.Verify(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnEmailAlreadyExistsError()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            UserName = "testuser",
            Password = "SecurePass123!"
        };

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync("existing@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Error.Should().Be(AuthErrorCodes.EmailAlreadyExists);
        result.Error.Message.Should().Contain("already registered");

        _userRepositoryMock.Verify(r => r.EmailExistsAsync("existing@example.com", It.IsAny<CancellationToken>()), Times.Once);
        _roleRepositoryMock.Verify(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _passwordHashingServiceMock.Verify(s => s.HashPassword(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_WithMissingDefaultRole_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            UserName = "testuser",
            Password = "SecurePass123!"
        };

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _roleRepositoryMock
            .Setup(r => r.GetByNameAsync("User", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _authService.RegisterAsync(request));

        _roleRepositoryMock.Verify(r => r.GetByNameAsync("User", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("", "testuser", "SecurePass123!")]
    [InlineData("   ", "testuser", "SecurePass123!")]
    [InlineData("test@example.com", "", "SecurePass123!")]
    [InlineData("test@example.com", "   ", "SecurePass123!")]
    public async Task RegisterAsync_WithInvalidData_ShouldStillAttemptRegistration(string email, string userName, string password)
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = email,
            UserName = userName,
            Password = password
        };

        var defaultRole = new Role { Id = Guid.NewGuid(), Name = "User" };

        _userRepositoryMock
            .Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _roleRepositoryMock
            .Setup(r => r.GetByNameAsync("User", It.IsAny<CancellationToken>()))
            .ReturnsAsync(defaultRole);

        _passwordHashingServiceMock
            .Setup(s => s.HashPassword(It.IsAny<string>()))
            .Returns((new byte[256], new byte[128]));

        _userRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User user, CancellationToken _) => user);

        _auditLogRepositoryMock
            .Setup(r => r.AddAsync(It.IsAny<AuditLog>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RegisterAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        // The service should normalize the email and username by trimming
        _userRepositoryMock.Verify(r => r.EmailExistsAsync(
            It.Is<string>(e => e == email.Trim().ToLowerInvariant()),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Login Tests

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnLoginResponse()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "SecurePass123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            UserName = "testuser",
            PasswordHash = new byte[256],
            PasswordSalt = new byte[128],
            CreatedAt = DateTime.UtcNow,
            UserRoles = new List<UserRole>
            {
                new UserRole { Role = new Role { Name = "User" } }
            }
        };

        var accessToken = "access_token";
        var refreshToken = "refresh_token";

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHashingServiceMock
            .Setup(s => s.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            .Returns(true);

        _jwtTokenServiceMock
            .Setup(s => s.GenerateAccessToken(user))
            .Returns(accessToken);

        _jwtTokenServiceMock
            .Setup(s => s.GenerateRefreshToken(user.Id))
            .Returns(refreshToken);

        _jwtTokenServiceMock
            .Setup(s => s.GetAccessTokenExpiresIn())
            .Returns(3600);

        _jwtTokenServiceMock
            .Setup(s => s.GetRefreshTokenExpiresInDays())
            .Returns(7);

        _userRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be(accessToken);
        result.Value.RefreshToken.Should().Be(refreshToken);
        result.Value.ExpiresIn.Should().Be(3600);
        result.Value.User.Should().NotBeNull();
        result.Value.User!.Id.Should().Be(user.Id);
        result.Value.User.Email.Should().Be(user.Email);
        result.Value.User.UserName.Should().Be(user.UserName);
        result.Value.User.Roles.Should().Contain("User");

        _userRepositoryMock.Verify(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()), Times.Once);
        _passwordHashingServiceMock.Verify(s => s.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt), Times.Once);
        _jwtTokenServiceMock.Verify(s => s.GenerateAccessToken(user), Times.Once);
        _jwtTokenServiceMock.Verify(s => s.GenerateRefreshToken(user.Id), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.RefreshToken == refreshToken), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WithNonExistentUser_ShouldReturnInvalidCredentialsError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "nonexistent@example.com",
            Password = "SecurePass123!"
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("nonexistent@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Error.Should().Be(AuthErrorCodes.InvalidCredentials);
        result.Error.Message.Should().Be("Invalid email or password");

        _userRepositoryMock.Verify(r => r.GetByEmailAsync("nonexistent@example.com", It.IsAny<CancellationToken>()), Times.Once);
        _passwordHashingServiceMock.Verify(s => s.VerifyPassword(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnInvalidCredentialsError()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "WrongPassword123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = new byte[256],
            PasswordSalt = new byte[128]
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHashingServiceMock
            .Setup(s => s.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            .Returns(false);

        // Act
        var result = await _authService.LoginAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Error.Should().Be(AuthErrorCodes.InvalidCredentials);
        result.Error.Message.Should().Be("Invalid email or password");

        _passwordHashingServiceMock.Verify(s => s.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt), Times.Once);
        _jwtTokenServiceMock.Verify(s => s.GenerateAccessToken(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region Token Refresh Tests

    [Fact]
    public async Task RefreshTokenAsync_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "valid_refresh_token"
        };

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            RefreshToken = "valid_refresh_token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };

        var newAccessToken = "new_access_token";
        var newRefreshToken = "new_refresh_token";

        _jwtTokenServiceMock
            .Setup(s => s.ValidateRefreshToken(request.RefreshToken))
            .Returns(userId);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _jwtTokenServiceMock
            .Setup(s => s.GenerateAccessToken(user))
            .Returns(newAccessToken);

        _jwtTokenServiceMock
            .Setup(s => s.GenerateRefreshToken(user.Id))
            .Returns(newRefreshToken);

        _jwtTokenServiceMock
            .Setup(s => s.GetAccessTokenExpiresIn())
            .Returns(3600);

        _jwtTokenServiceMock
            .Setup(s => s.GetRefreshTokenExpiresInDays())
            .Returns(7);

        _userRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.RefreshTokenAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.AccessToken.Should().Be(newAccessToken);
        result.Value.RefreshToken.Should().Be(newRefreshToken);
        result.Value.ExpiresIn.Should().Be(3600);

        _jwtTokenServiceMock.Verify(s => s.ValidateRefreshToken(request.RefreshToken), Times.Once);
        _userRepositoryMock.Verify(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()), Times.Once);
        _jwtTokenServiceMock.Verify(s => s.GenerateAccessToken(user), Times.Once);
        _jwtTokenServiceMock.Verify(s => s.GenerateRefreshToken(user.Id), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithInvalidRefreshToken_ShouldReturnInvalidRefreshTokenError()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "invalid_refresh_token"
        };

        _jwtTokenServiceMock
            .Setup(s => s.ValidateRefreshToken(request.RefreshToken))
            .Returns((Guid?)null);

        // Act
        var result = await _authService.RefreshTokenAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Error.Should().Be(AuthErrorCodes.InvalidRefreshToken);
        result.Error.Message.Should().Be("Invalid refresh token");

        _jwtTokenServiceMock.Verify(s => s.ValidateRefreshToken(request.RefreshToken), Times.Once);
        _userRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredRefreshToken_ShouldReturnInvalidRefreshTokenError()
    {
        // Arrange
        var request = new RefreshTokenRequest
        {
            RefreshToken = "expired_refresh_token"
        };

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            RefreshToken = "expired_refresh_token",
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(-1) // Expired
        };

        _jwtTokenServiceMock
            .Setup(s => s.ValidateRefreshToken(request.RefreshToken))
            .Returns(userId);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.RefreshTokenAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Error.Should().Be(AuthErrorCodes.InvalidRefreshToken);
        result.Error.Message.Should().Be("Invalid or expired refresh token");
    }

    #endregion

    #region Logout Tests

    [Fact]
    public async Task LogoutAsync_WithValidRefreshToken_ShouldClearRefreshToken()
    {
        // Arrange
        var request = new LogoutRequest
        {
            RefreshToken = "valid_refresh_token"
        };

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            RefreshToken = "valid_refresh_token"
        };

        _jwtTokenServiceMock
            .Setup(s => s.ValidateRefreshToken(request.RefreshToken))
            .Returns(userId);

        _userRepositoryMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.LogoutAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.Message.Should().Be("Logged out successfully");

        _userRepositoryMock.Verify(r => r.UpdateAsync(
            It.Is<User>(u => u.RefreshToken == null && u.RefreshTokenExpiryTime == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Password Reset Tests

    [Fact]
    public async Task ResetPasswordAsync_WithValidToken_ShouldUpdatePassword()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "valid_reset_token",
            NewPassword = "NewSecurePass123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            PasswordHash = new byte[256],
            PasswordSalt = new byte[128],
            PasswordResetToken = "valid_reset_token",
            PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1)
        };

        var newHash = new byte[256];
        var newSalt = new byte[128];

        _userRepositoryMock
            .Setup(r => r.GetByPasswordResetTokenAsync(request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHashingServiceMock
            .Setup(s => s.VerifyPassword(request.NewPassword, user.PasswordHash, user.PasswordSalt))
            .Returns(false); // New password is different

        _passwordHashingServiceMock
            .Setup(s => s.HashPassword(request.NewPassword))
            .Returns((newHash, newSalt));

        _userRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.ResetPasswordAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Password reset successfully.");

        _userRepositoryMock.Verify(r => r.UpdateAsync(
            It.Is<User>(u =>
                u.PasswordHash == newHash &&
                u.PasswordSalt == newSalt &&
                u.PasswordResetToken == null &&
                u.PasswordResetTokenExpiry == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithExpiredToken_ShouldReturnTokenExpiredError()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "expired_reset_token",
            NewPassword = "NewSecurePass123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            PasswordResetToken = "expired_reset_token",
            PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(-1) // Expired
        };

        _userRepositoryMock
            .Setup(r => r.GetByPasswordResetTokenAsync(request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        // Act
        var result = await _authService.ResetPasswordAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Error.Should().Be(AuthErrorCodes.TokenExpired);
        result.Error.Message.Should().Be("Reset token has expired");

        _passwordHashingServiceMock.Verify(s => s.HashPassword(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ResetPasswordAsync_WithSamePassword_ShouldReturnSamePasswordError()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Token = "valid_reset_token",
            NewPassword = "SamePassword123!"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            PasswordHash = new byte[256],
            PasswordSalt = new byte[128],
            PasswordResetToken = "valid_reset_token",
            PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1)
        };

        _userRepositoryMock
            .Setup(r => r.GetByPasswordResetTokenAsync(request.Token, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _passwordHashingServiceMock
            .Setup(s => s.VerifyPassword(request.NewPassword, user.PasswordHash, user.PasswordSalt))
            .Returns(true); // Same password

        // Act
        var result = await _authService.ResetPasswordAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Error.Should().Be(AuthErrorCodes.SamePassword);
        result.Error.Message.Should().Be("New password cannot be the same as the current password");

        _passwordHashingServiceMock.Verify(s => s.HashPassword(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Forgot Password Tests

    [Fact]
    public async Task ForgotPasswordAsync_WithExistingUser_ShouldGenerateResetTokenAndSendEmail()
    {
        // Arrange
        var request = new ForgotPasswordRequest
        {
            Email = "test@example.com"
        };

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com"
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _userRepositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _emailServiceMock
            .Setup(s => s.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _authService.ForgotPasswordAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("If an account with this email exists, a password reset link has been sent.");

        _userRepositoryMock.Verify(r => r.GetByEmailAsync("test@example.com", It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.Is<User>(u => u.PasswordResetToken != null), It.IsAny<CancellationToken>()), Times.Once);
        _emailServiceMock.Verify(s => s.SendPasswordResetEmailAsync(user.Email, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ForgotPasswordAsync_WithNonExistentUser_ShouldReturnGenericMessage()
    {
        // Arrange
        var request = new ForgotPasswordRequest
        {
            Email = "nonexistent@example.com"
        };

        _userRepositoryMock
            .Setup(r => r.GetByEmailAsync("nonexistent@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        // Act
        var result = await _authService.ForgotPasswordAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("If an account with this email exists, a password reset link has been sent.");

        _userRepositoryMock.Verify(r => r.GetByEmailAsync("nonexistent@example.com", It.IsAny<CancellationToken>()), Times.Once);
        _userRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailServiceMock.Verify(s => s.SendPasswordResetEmailAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    #endregion
}
