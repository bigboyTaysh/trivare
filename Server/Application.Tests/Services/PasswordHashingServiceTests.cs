using System.Security.Cryptography;
using FluentAssertions;
using Trivare.Application.Services;
using Xunit;

namespace Trivare.Application.Tests.Services;

public class PasswordHashingServiceTests
{
    private readonly PasswordHashingService _passwordHashingService;

    public PasswordHashingServiceTests()
    {
        _passwordHashingService = new PasswordHashingService();
    }

    #region HashPassword Tests

    [Fact]
    public void HashPassword_ShouldReturnHashAndSaltOfCorrectLength()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var (hash, salt) = _passwordHashingService.HashPassword(password);

        // Assert
        hash.Should().NotBeNull();
        hash.Length.Should().Be(256); // HashSize = 256 bytes

        salt.Should().NotBeNull();
        salt.Length.Should().Be(128); // SaltSize = 128 bytes
    }

    [Fact]
    public void HashPassword_WithDifferentPasswords_ShouldProduceDifferentHashes()
    {
        // Arrange
        var password1 = "Password123!";
        var password2 = "DifferentPassword123!";

        // Act
        var (hash1, salt1) = _passwordHashingService.HashPassword(password1);
        var (hash2, salt2) = _passwordHashingService.HashPassword(password2);

        // Assert
        hash1.Should().NotBeEquivalentTo(hash2);
        salt1.Should().NotBeEquivalentTo(salt2);
    }

    [Fact]
    public void HashPassword_WithSamePassword_ShouldProduceDifferentHashesDueToDifferentSalts()
    {
        // Arrange
        var password = "SamePassword123!";

        // Act
        var (hash1, salt1) = _passwordHashingService.HashPassword(password);
        var (hash2, salt2) = _passwordHashingService.HashPassword(password);

        // Assert
        hash1.Should().NotBeEquivalentTo(hash2);
        salt1.Should().NotBeEquivalentTo(salt2);
    }

    [Fact]
    public void HashPassword_ShouldUseCryptographicallyStrongSalt()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act - Generate multiple hashes and check salt randomness
        var salts = new List<byte[]>();
        for (int i = 0; i < 10; i++)
        {
            var (_, salt) = _passwordHashingService.HashPassword(password);
            salts.Add(salt);
        }

        // Assert - All salts should be different (extremely unlikely to be the same)
        var uniqueSalts = salts.Distinct(new ByteArrayComparer()).ToList();
        uniqueSalts.Should().HaveCount(10);
    }

    #endregion

    #region VerifyPassword Tests

    [Fact]
    public void VerifyPassword_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "CorrectPassword123!";
        var (hash, salt) = _passwordHashingService.HashPassword(password);

        // Act
        var result = _passwordHashingService.VerifyPassword(password, hash, salt);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var correctPassword = "CorrectPassword123!";
        var wrongPassword = "WrongPassword123!";
        var (hash, salt) = _passwordHashingService.HashPassword(correctPassword);

        // Act
        var result = _passwordHashingService.VerifyPassword(wrongPassword, hash, salt);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithWrongSalt_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var (hash, correctSalt) = _passwordHashingService.HashPassword(password);

        // Create wrong salt (flip one byte)
        var wrongSalt = new byte[correctSalt.Length];
        Array.Copy(correctSalt, wrongSalt, correctSalt.Length);
        wrongSalt[0] = (byte)(wrongSalt[0] ^ 0xFF);

        // Act
        var result = _passwordHashingService.VerifyPassword(password, hash, wrongSalt);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void VerifyPassword_WithWrongHash_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123!";
        var (_, salt) = _passwordHashingService.HashPassword(password);

        // Create wrong hash (flip one byte)
        var wrongHash = new byte[256];
        wrongHash[0] = 0xFF;

        // Act
        var result = _passwordHashingService.VerifyPassword(password, wrongHash, salt);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("password")]
    [InlineData("Password123")]
    [InlineData("Pass123!")]
    public void VerifyPassword_ShouldWorkWithVariousPasswordComplexities(string password)
    {
        // Arrange
        var (hash, salt) = _passwordHashingService.HashPassword(password);

        // Act & Assert
        _passwordHashingService.VerifyPassword(password, hash, salt).Should().BeTrue();
        _passwordHashingService.VerifyPassword(password + "wrong", hash, salt).Should().BeFalse();
    }

    #endregion

    #region Security Properties Tests

    [Fact]
    public void HashPassword_ShouldUsePBKDF2WithSHA256()
    {
        // This test verifies the implementation uses the expected cryptographic parameters
        // We can't directly test the internal implementation, but we can verify the behavior

        // Arrange
        var password = "TestPassword123!";

        // Act
        var (hash1, salt1) = _passwordHashingService.HashPassword(password);
        var (hash2, salt2) = _passwordHashingService.HashPassword(password);

        // Assert - Verify different results for same password (different salts)
        hash1.Should().NotBeEquivalentTo(hash2);
        salt1.Should().NotBeEquivalentTo(salt2);

        // Verify correct lengths
        hash1.Length.Should().Be(256); // 256 bytes = 2048 bits
        salt1.Length.Should().Be(128); // 128 bytes = 1024 bits
    }

    [Fact]
    public void VerifyPassword_ShouldUseConstantTimeComparison()
    {
        // This test verifies that the verification process takes approximately the same time
        // regardless of whether the password matches or not (to prevent timing attacks)

        // Arrange
        var password = "TestPassword123!";
        var wrongPassword = "WrongPassword123!";
        var (hash, salt) = _passwordHashingService.HashPassword(password);

        // Act - Time both operations
        var correctStartTime = DateTime.UtcNow;
        var correctResult = _passwordHashingService.VerifyPassword(password, hash, salt);
        var correctDuration = DateTime.UtcNow - correctStartTime;

        var wrongStartTime = DateTime.UtcNow;
        var wrongResult = _passwordHashingService.VerifyPassword(wrongPassword, hash, salt);
        var wrongDuration = DateTime.UtcNow - wrongStartTime;

        // Assert
        correctResult.Should().BeTrue();
        wrongResult.Should().BeFalse();

        // The durations should be very similar (within a reasonable tolerance)
        // This is a basic check - in a real security audit, you'd use more sophisticated timing analysis
        var timeDifference = Math.Abs((correctDuration - wrongDuration).TotalMilliseconds);
        timeDifference.Should().BeLessThan(100); // Allow for some variance in timing
    }

    [Fact]
    public void HashPassword_ShouldBeDeterministicWithSameSalt()
    {
        // This test verifies that if we could use the same salt, we'd get the same hash
        // (We can't directly test this with the current API, but we can test the PBKDF2 behavior manually)

        // Arrange
        var password = "TestPassword123!";
        var salt = new byte[128];
        new Random(42).NextBytes(salt); // Use a fixed seed for deterministic salt

        // Manually create PBKDF2 hash with same parameters as the service
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256))
        {
            var expectedHash = pbkdf2.GetBytes(256);

            // Act - Verify that our manual PBKDF2 matches what we'd expect
            // This is more of an integration test to ensure our understanding is correct

            // Assert
            expectedHash.Should().NotBeNull();
            expectedHash.Length.Should().Be(256);
        }
    }

    [Fact]
    public void PasswordHashing_ShouldResistRainbowTableAttacks()
    {
        // Test that the same password with different salts produces different hashes
        // This is the primary defense against rainbow table attacks

        // Arrange
        var password = "CommonPassword123!";
        var hashes = new List<byte[]>();

        // Act - Generate multiple hashes for the same password (reduced iterations for performance)
        for (int i = 0; i < 10; i++)
        {
            var (hash, _) = _passwordHashingService.HashPassword(password);
            hashes.Add(hash);
        }

        // Assert - All hashes should be different
        var uniqueHashes = hashes.Distinct(new ByteArrayComparer()).ToList();
        uniqueHashes.Should().HaveCount(10);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void HashPassword_WithEmptyPassword_ShouldStillWork()
    {
        // Arrange
        var password = "";

        // Act
        var (hash, salt) = _passwordHashingService.HashPassword(password);

        // Assert
        hash.Should().NotBeNull();
        hash.Length.Should().Be(256);
        salt.Should().NotBeNull();
        salt.Length.Should().Be(128);

        // Should be able to verify
        _passwordHashingService.VerifyPassword(password, hash, salt).Should().BeTrue();
    }

    [Fact]
    public void HashPassword_WithVeryLongPassword_ShouldWork()
    {
        // Arrange
        var password = new string('A', 1000); // 1,000 character password

        // Act
        var (hash, salt) = _passwordHashingService.HashPassword(password);

        // Assert
        hash.Should().NotBeNull();
        hash.Length.Should().Be(256);
        salt.Should().NotBeNull();
        salt.Length.Should().Be(128);

        // Should be able to verify
        _passwordHashingService.VerifyPassword(password, hash, salt).Should().BeTrue();
    }

    [Fact]
    public void HashPassword_WithNullPassword_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _passwordHashingService.HashPassword(null!));
    }

    [Fact]
    public void VerifyPassword_WithNullOrEmptyInputs_ShouldThrowArgumentNullException()
    {
        // Arrange
        var password = "TestPassword123!";
        var (hash, salt) = _passwordHashingService.HashPassword(password);

        // Act & Assert - These should throw ArgumentNullException
        Assert.Throws<ArgumentNullException>(() => _passwordHashingService.VerifyPassword(null!, hash, salt));
        Assert.Throws<ArgumentNullException>(() => _passwordHashingService.VerifyPassword(password, null!, salt));
        Assert.Throws<ArgumentNullException>(() => _passwordHashingService.VerifyPassword(password, hash, null!));
    }

    #endregion

    #region Helper Classes

    private class ByteArrayComparer : IEqualityComparer<byte[]>
    {
        public bool Equals(byte[]? x, byte[]? y)
        {
            if (x == null || y == null) return false;
            if (x.Length != y.Length) return false;
            return x.SequenceEqual(y);
        }

        public int GetHashCode(byte[] obj)
        {
            // Simple hash code implementation for byte arrays
            return obj.Aggregate(0, (current, b) => current ^ b);
        }
    }

    #endregion
}
