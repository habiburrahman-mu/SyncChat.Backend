using FluentAssertions;
using SyncChat.API.Infrastructure.Security;
using System.Diagnostics.CodeAnalysis;

namespace SyncChat.Test.Infrastructure;

[ExcludeFromCodeCoverage(
    Justification = "This is a test class and does not require coverage."
)]
public class PasswordHasherTests
{
    private readonly PasswordHasher passwordHasher;

    public PasswordHasherTests()
    {
        passwordHasher = new PasswordHasher();
    }

    #region Hash Tests

    [Theory(DisplayName = "Hash should throw ArgumentException when password is empty or whitespace")]
    [InlineData("")]
    [InlineData("    ")]
    public void Hash_ShouldThrowArgumentException_WhenPasswordEmptyOrWhiteSpace(string password)
    {
        // Act
        Action act = () => passwordHasher.Hash(password);

        // Assert
        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Password cannot be null or empty.*")
            .WithParameterName("password");
    }

    [Fact(DisplayName = "Hash should return a valid hash when password is valid")]
    public void Hash_ShouldReturnValidHash_WhenPasswordIsValid()
    {
        // Arrange
        string password = "ValidPassword@123";

        // Act
        string result = passwordHasher.Hash(password);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().Contain("-");
        result.Split("-").Length.Should().Be(2);
    }

    #endregion Hash Tests

    #region Verify Tests

    [Fact(DisplayName = "Verify should return true when password and hash match")]
    public void Verify_ShouldReturnTrue_WhenPasswordAndHashMatch()
    {
        // Arrange
        string password = "ValidPassword@123";
        string hash = passwordHasher.Hash(password);
        
        // Act
        bool result = passwordHasher.Verify(password, hash);
        
        // Assert
        result.Should().BeTrue();
    }

    [Fact(DisplayName = "Verify should return false when password does not match hash")]
    public void Verify_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        // Arrange
        string password = "ValidPassword@123";

        string wrongPassword = "WrongPassword@456";
        
        string hash = passwordHasher.Hash(password);
        
        // Act
        bool result = passwordHasher.Verify(wrongPassword, hash);
        
        // Assert
        result.Should().BeFalse();
    }

    [Theory(DisplayName = "Verify should return false when password is empty or whitespace")]
    [InlineData("")]
    [InlineData("   ")]
    public void Verify_ShouldReturnFalse_WhenPasswordIsInvalid(string invalidPassword)
    {
        // Arrange
        string hashedPassword = passwordHasher.Hash("ValidPassword@123");

        // Act
        bool result = passwordHasher.Verify(invalidPassword, hashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Theory(DisplayName = "Verify should return false when hash is empty or whitespace")]
    [InlineData("")]
    [InlineData("   ")]
    public void Verify_ShouldReturnFalse_WhenHashedPasswordIsInvalid(string invalidHash)
    {
        // Arrange
        string password = "ValidPassword@123";

        // Act
        bool result = passwordHasher.Verify(password, invalidHash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Verify should return false when hashed password has invalid format")]
    public void Verify_ShouldReturnFalse_WhenHashedPasswordHasInvalidFormat()
    {
        // Arrange
        string password = "ValidPassword@123";
        string invalidHashedPassword = "invalid-format-without-split";

        // Act
        bool result = passwordHasher.Verify(password, invalidHashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Hash should generate different hashes for same password due to random salt")]
    public void Hash_ShouldGenerateDifferentHashes_ForSamePassword()
    {
        // Arrange
        string password = "RepeatedPassword";

        // Act
        string hash1 = passwordHasher.Hash(password);
        string hash2 = passwordHasher.Hash(password);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    #endregion Verify Tests
}
