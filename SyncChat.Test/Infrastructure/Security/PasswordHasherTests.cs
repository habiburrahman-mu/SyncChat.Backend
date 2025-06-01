using FluentAssertions;
using SyncChat.API.Infrastructure.Security;
using System.Diagnostics.CodeAnalysis;

namespace SyncChat.Test.Infrastructure.Security;

[ExcludeFromCodeCoverage(
    Justification = "This is a test class and does not require coverage."
)]
public class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher;

    public PasswordHasherTests()
    {
        _passwordHasher = new PasswordHasher();
    }

    #region Hash Tests

    [Theory(DisplayName = "Hash should throw ArgumentException when password is empty or whitespace")]
    [InlineData("")]
    [InlineData("    ")]
    public void Hash_ShouldThrowArgumentException_WhenPasswordEmptyOrWhiteSpace(string password)
    {
        // Act
        Action act = () => _passwordHasher.Hash(password);

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
        string result = _passwordHasher.Hash(password);

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
        string hash = _passwordHasher.Hash(password);
        
        // Act
        bool result = _passwordHasher.Verify(password, hash);
        
        // Assert
        result.Should().BeTrue();
    }

    [Fact(DisplayName = "Verify should return false when password does not match hash")]
    public void Verify_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
    {
        // Arrange
        string password = "ValidPassword@123";

        string wrongPassword = "WrongPassword@456";
        
        string hash = _passwordHasher.Hash(password);
        
        // Act
        bool result = _passwordHasher.Verify(wrongPassword, hash);
        
        // Assert
        result.Should().BeFalse();
    }

    [Theory(DisplayName = "Verify should return false when password is empty or whitespace")]
    [InlineData("")]
    [InlineData("   ")]
    public void Verify_ShouldReturnFalse_WhenPasswordIsInvalid(string invalidPassword)
    {
        // Arrange
        string hashedPassword = _passwordHasher.Hash("ValidPassword@123");

        // Act
        bool result = _passwordHasher.Verify(invalidPassword, hashedPassword);

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
        bool result = _passwordHasher.Verify(password, invalidHash);

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
        bool result = _passwordHasher.Verify(password, invalidHashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Hash should generate different hashes for same password due to random salt")]
    public void Hash_ShouldGenerateDifferentHashes_ForSamePassword()
    {
        // Arrange
        string password = "RepeatedPassword";

        // Act
        string hash1 = _passwordHasher.Hash(password);
        string hash2 = _passwordHasher.Hash(password);

        // Assert
        hash1.Should().NotBe(hash2);
    }

    [Fact(DisplayName = "Verify should return false when hashed password parts are not valid hex")]
    public void Verify_ShouldReturnFalse_WhenHashedPasswordIsInvalidHex()
    {
        // Arrange
        string password = "SomePassword123!";
        string invalidHashedPassword = "NotAHexString-EitherThis";

        // Act
        bool result = _passwordHasher.Verify(password, invalidHashedPassword);

        // Assert
        result.Should().BeFalse();
    }

    #endregion Verify Tests
}
