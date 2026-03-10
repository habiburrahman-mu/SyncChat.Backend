using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Configuration;
using System.Diagnostics.CodeAnalysis;

namespace SyncChat.Test.Infrastructure.Security;

[ExcludeFromCodeCoverage(Justification = "This is a test class and does not require coverage.")]
public class CookieOptionsProviderTests
{
    private readonly Mock<IWebHostEnvironment> _mockEnvironment;
    private readonly JWTSettings _jwtSettings;

    public CookieOptionsProviderTests()
    {
        _mockEnvironment = new Mock<IWebHostEnvironment>();
        _jwtSettings = new JWTSettings
        {
            Issuer = "test-issuer",
            Audience = "test-audience",
            AccessTokenSecret = "test-secret-key-that-is-long-enough-for-hmac-sha256",
            AccessTokenExpirationInMinutes = 15,
            RefreshTokenExpirationInMinutes = 10080 // 7 days
        };
    }

    #region CreateRefreshTokenOptions Tests

    [Fact(DisplayName = "CreateRefreshTokenOptions should return HttpOnly cookie")]
    public void CreateRefreshTokenOptions_ShouldReturnHttpOnlyCookie()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var provider = CreateProvider();

        // Act
        var options = provider.CreateRefreshTokenOptions();

        // Assert
        options.HttpOnly.Should().BeTrue();
    }

    [Fact(DisplayName = "CreateRefreshTokenOptions should set Secure to false in Development")]
    public void CreateRefreshTokenOptions_ShouldSetSecureToFalse_InDevelopment()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var provider = CreateProvider();

        // Act
        var options = provider.CreateRefreshTokenOptions();

        // Assert
        options.Secure.Should().BeFalse();
    }

    [Fact(DisplayName = "CreateRefreshTokenOptions should set Secure to true in Production")]
    public void CreateRefreshTokenOptions_ShouldSetSecureToTrue_InProduction()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        var provider = CreateProvider();

        // Act
        var options = provider.CreateRefreshTokenOptions();

        // Assert
        options.Secure.Should().BeTrue();
    }

    [Fact(DisplayName = "CreateRefreshTokenOptions should set Secure to true in Staging")]
    public void CreateRefreshTokenOptions_ShouldSetSecureToTrue_InStaging()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Staging);
        var provider = CreateProvider();

        // Act
        var options = provider.CreateRefreshTokenOptions();

        // Assert
        options.Secure.Should().BeTrue();
    }

    [Fact(DisplayName = "CreateRefreshTokenOptions should set SameSite to Lax in Development")]
    public void CreateRefreshTokenOptions_ShouldSetSameSiteToLax_InDevelopment()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var provider = CreateProvider();

        // Act
        var options = provider.CreateRefreshTokenOptions();

        // Assert
        options.SameSite.Should().Be(SameSiteMode.Lax);
    }

    [Fact(DisplayName = "CreateRefreshTokenOptions should set SameSite to None in Production")]
    public void CreateRefreshTokenOptions_ShouldSetSameSiteToNone_InProduction()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Production);
        var provider = CreateProvider();

        // Act
        var options = provider.CreateRefreshTokenOptions();

        // Assert
        options.SameSite.Should().Be(SameSiteMode.None);
    }

    [Fact(DisplayName = "CreateRefreshTokenOptions should set expiration from JWT settings")]
    public void CreateRefreshTokenOptions_ShouldSetExpiration_FromJwtSettings()
    {
        // Arrange
        DateTime beforeCreation = DateTime.UtcNow;
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var provider = CreateProvider();

        // Act
        var options = provider.CreateRefreshTokenOptions();
        DateTime afterCreation = DateTime.UtcNow;

        // Assert
        options.Expires.Should().NotBeNull();
        
        var expectedMinExpiry = beforeCreation.AddMinutes(_jwtSettings.RefreshTokenExpirationInMinutes);
        var expectedMaxExpiry = afterCreation.AddMinutes(_jwtSettings.RefreshTokenExpirationInMinutes);
        
        options.Expires.Should().BeOnOrAfter(expectedMinExpiry);
        options.Expires.Should().BeOnOrBefore(expectedMaxExpiry);
    }

    [Fact(DisplayName = "CreateRefreshTokenOptions should create new options on each call")]
    public void CreateRefreshTokenOptions_ShouldCreateNewOptions_OnEachCall()
    {
        // Arrange
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var provider = CreateProvider();

        // Act
        var options1 = provider.CreateRefreshTokenOptions();
        System.Threading.Thread.Sleep(10); // Small delay to ensure different timestamps
        var options2 = provider.CreateRefreshTokenOptions();

        // Assert
        options1.Should().NotBeSameAs(options2);
        options1.Expires.Should().NotBe(options2.Expires);
    }

    [Theory(DisplayName = "CreateRefreshTokenOptions should respect different expiration settings")]
    [InlineData(60)]     // 1 hour
    [InlineData(1440)]   // 1 day
    [InlineData(10080)]  // 7 days
    [InlineData(43200)]  // 30 days
    public void CreateRefreshTokenOptions_ShouldRespectDifferentExpirationSettings(int expirationInMinutes)
    {
        // Arrange
        DateTime beforeCreation = DateTime.UtcNow;
        _jwtSettings.RefreshTokenExpirationInMinutes = expirationInMinutes;
        _mockEnvironment.Setup(e => e.EnvironmentName).Returns(Environments.Development);
        var provider = CreateProvider();

        // Act
        var options = provider.CreateRefreshTokenOptions();
        DateTime afterCreation = DateTime.UtcNow;

        // Assert
        var expectedMinExpiry = beforeCreation.AddMinutes(expirationInMinutes);
        var expectedMaxExpiry = afterCreation.AddMinutes(expirationInMinutes);
        
        options.Expires.Should().BeOnOrAfter(expectedMinExpiry);
        options.Expires.Should().BeOnOrBefore(expectedMaxExpiry);
    }

    #endregion

    private CookieOptionsProvider CreateProvider()
    {
        return new CookieOptionsProvider(
            _mockEnvironment.Object,
            Options.Create(_jwtSettings));
    }
}
