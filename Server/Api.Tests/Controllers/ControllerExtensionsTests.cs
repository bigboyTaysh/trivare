using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Trivare.Api.Extensions;
using Xunit;

namespace Trivare.Api.Tests.Controllers;

public class ControllerExtensionsTests
{
    private readonly Mock<ControllerBase> _controllerMock;

    public ControllerExtensionsTests()
    {
        _controllerMock = new Mock<ControllerBase>();
    }

    #region GetAuthenticatedUserId Tests

    [Fact]
    public void GetAuthenticatedUserId_WithValidGuidClaim_ShouldReturnUserId()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controllerMock.Object.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = _controllerMock.Object.GetAuthenticatedUserId();

        // Assert
        result.Should().Be(expectedUserId);
    }

    [Fact]
    public void GetAuthenticatedUserId_WithInvalidGuidClaim_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "invalid-guid-string")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controllerMock.Object.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(
            () => _controllerMock.Object.GetAuthenticatedUserId());

        exception.Message.Should().Be("Invalid or missing user ID in authentication token");
    }

    [Fact]
    public void GetAuthenticatedUserId_WithMissingNameIdentifierClaim_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Email, "test@example.com") // Different claim type
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controllerMock.Object.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(
            () => _controllerMock.Object.GetAuthenticatedUserId());

        exception.Message.Should().Be("Invalid or missing user ID in authentication token");
    }

    [Fact]
    public void GetAuthenticatedUserId_WithNullUser_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        _controllerMock.Object.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = null }
        };

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(
            () => _controllerMock.Object.GetAuthenticatedUserId());

        exception.Message.Should().Be("Invalid or missing user ID in authentication token");
    }

    [Fact]
    public void GetAuthenticatedUserId_WithEmptyNameIdentifierClaim_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controllerMock.Object.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(
            () => _controllerMock.Object.GetAuthenticatedUserId());

        exception.Message.Should().Be("Invalid or missing user ID in authentication token");
    }

    [Fact]
    public void GetAuthenticatedUserId_WithWhitespaceNameIdentifierClaim_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "   ")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controllerMock.Object.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act & Assert
        var exception = Assert.Throws<UnauthorizedAccessException>(
            () => _controllerMock.Object.GetAuthenticatedUserId());

        exception.Message.Should().Be("Invalid or missing user ID in authentication token");
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void GetAuthenticatedUserId_WithMultipleNameIdentifierClaims_ShouldUseFirstClaim()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, anotherUserId.ToString()) // Second claim should be ignored
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controllerMock.Object.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = _controllerMock.Object.GetAuthenticatedUserId();

        // Assert
        result.Should().Be(expectedUserId);
    }

    [Fact]
    public void GetAuthenticatedUserId_WithCaseInsensitiveClaimType_ShouldWork()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim("nameidentifier", expectedUserId.ToString()) // lowercase
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controllerMock.Object.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = _controllerMock.Object.GetAuthenticatedUserId();

        // Assert
        result.Should().Be(expectedUserId);
    }

    [Fact]
    public void GetAuthenticatedUserId_WithGuidWithHyphens_ShouldParseCorrectly()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString("D")) // Standard format with hyphens
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controllerMock.Object.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = _controllerMock.Object.GetAuthenticatedUserId();

        // Assert
        result.Should().Be(expectedUserId);
    }

    [Fact]
    public void GetAuthenticatedUserId_WithGuidWithoutHyphens_ShouldParseCorrectly()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString("N")) // Format without hyphens
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);

        _controllerMock.Object.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = _controllerMock.Object.GetAuthenticatedUserId();

        // Assert
        result.Should().Be(expectedUserId);
    }

    #endregion
}
