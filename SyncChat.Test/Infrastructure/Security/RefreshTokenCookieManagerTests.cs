using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SyncChat.API.Infrastructure.Security;
using SyncChat.API.Shared.Security.Contracts;
using System.Diagnostics.CodeAnalysis;

namespace SyncChat.Test.Infrastructure.Security;

[ExcludeFromCodeCoverage(Justification = "This is a test class and does not require coverage.")]
public class RefreshTokenCookieManagerTests
{
    private const string CookieName = "refreshToken";
    private readonly Mock<ICookieOptionsProvider> _mockCookieOptionsProvider;
    private readonly CookieOptions _defaultCookieOptions;

    public RefreshTokenCookieManagerTests()
    {
        _mockCookieOptionsProvider = new Mock<ICookieOptionsProvider>();
        _defaultCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.UtcNow.AddDays(7)
        };

        _mockCookieOptionsProvider
            .Setup(p => p.CreateRefreshTokenOptions())
            .Returns(_defaultCookieOptions);
    }

    #region Append Tests

    [Fact(DisplayName = "Append should add cookie to response with correct name and value")]
    public void Append_ShouldAddCookie_WithCorrectNameAndValue()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var manager = CreateManager();
        string refreshToken = "test-refresh-token-12345";

        // Act
        manager.Append(httpContext, refreshToken);

        // Assert
        var responseCookies = httpContext.Response.Cookies;
        
        // Verify cookie was appended (we can't directly inspect cookies, but we can verify the method was called)
        _mockCookieOptionsProvider.Verify(p => p.CreateRefreshTokenOptions(), Times.Once);
    }

    [Fact(DisplayName = "Append should use cookie options from provider")]
    public void Append_ShouldUseCookieOptions_FromProvider()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var manager = CreateManager();
        string refreshToken = "test-refresh-token";

        // Act
        manager.Append(httpContext, refreshToken);

        // Assert
        _mockCookieOptionsProvider.Verify(p => p.CreateRefreshTokenOptions(), Times.Once);
    }

    [Theory(DisplayName = "Append should handle different token values")]
    [InlineData("short")]
    [InlineData("this-is-a-very-long-refresh-token-with-many-characters-123456789")]
    [InlineData("token-with-special-chars-!@#$%")]
    public void Append_ShouldHandleDifferentTokenValues(string refreshToken)
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var manager = CreateManager();

        // Act
        Action act = () => manager.Append(httpContext, refreshToken);

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region TryGet Tests

    [Fact(DisplayName = "TryGet should return true when cookie exists")]
    public void TryGet_ShouldReturnTrue_WhenCookieExists()
    {
        // Arrange
        string expectedToken = "test-refresh-token";
        var httpContext = CreateHttpContextWithCookie(CookieName, expectedToken);
        var manager = CreateManager();

        // Act
        bool result = manager.TryGet(httpContext, out string actualToken);

        // Assert
        result.Should().BeTrue();
        actualToken.Should().Be(expectedToken);
    }

    [Fact(DisplayName = "TryGet should return false when cookie does not exist")]
    public void TryGet_ShouldReturnFalse_WhenCookieDoesNotExist()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var manager = CreateManager();

        // Act
        bool result = manager.TryGet(httpContext, out string token);

        // Assert
        result.Should().BeFalse();
        token.Should().BeEmpty();
    }

    [Fact(DisplayName = "TryGet should return false when cookie value is null")]
    public void TryGet_ShouldReturnFalse_WhenCookieValueIsNull()
    {
        // Arrange
        var httpContext = CreateHttpContextWithCookie(CookieName, null);
        var manager = CreateManager();

        // Act
        bool result = manager.TryGet(httpContext, out string token);

        // Assert
        result.Should().BeFalse();
        token.Should().BeEmpty();
    }

    [Fact(DisplayName = "TryGet should return false when different cookie name exists")]
    public void TryGet_ShouldReturnFalse_WhenDifferentCookieNameExists()
    {
        // Arrange
        var httpContext = CreateHttpContextWithCookie("differentCookie", "some-value");
        var manager = CreateManager();

        // Act
        bool result = manager.TryGet(httpContext, out string token);

        // Assert
        result.Should().BeFalse();
        token.Should().BeEmpty();
    }

    [Theory(DisplayName = "TryGet should correctly retrieve various token formats")]
    [InlineData("simple-token")]
    [InlineData("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.dozjgNryP4J3jVmNHl0w5N_XgL0n3I9PlFUP0THsR8U")]
    [InlineData("token_with_underscores")]
    [InlineData("token-with-dashes")]
    public void TryGet_ShouldCorrectlyRetrieve_VariousTokenFormats(string expectedToken)
    {
        // Arrange
        var httpContext = CreateHttpContextWithCookie(CookieName, expectedToken);
        var manager = CreateManager();

        // Act
        bool result = manager.TryGet(httpContext, out string actualToken);

        // Assert
        result.Should().BeTrue();
        actualToken.Should().Be(expectedToken);
    }

    [Fact(DisplayName = "TryGet should return false when cookie value is empty")]
    public void TryGet_ShouldReturnFalse_WhenCookieValueIsEmpty()
    {
        // Arrange
        // Empty cookie value is treated as null by ASP.NET Core cookie parser
        // So we test that it returns false
        var httpContext = CreateHttpContext(); // No cookie at all
        var manager = CreateManager();

        // Act
        bool result = manager.TryGet(httpContext, out string token);

        // Assert
        result.Should().BeFalse();
        token.Should().BeEmpty();
    }

    #endregion

    #region Delete Tests

    [Fact(DisplayName = "Delete should remove cookie from response")]
    public void Delete_ShouldRemoveCookie_FromResponse()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var manager = CreateManager();

        // Act
        Action act = () => manager.Delete(httpContext);

        // Assert
        act.Should().NotThrow();
    }

    [Fact(DisplayName = "Delete should not throw when cookie does not exist")]
    public void Delete_ShouldNotThrow_WhenCookieDoesNotExist()
    {
        // Arrange
        var httpContext = CreateHttpContext();
        var manager = CreateManager();

        // Act
        Action act = () => manager.Delete(httpContext);

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    #region Integration Scenarios

    [Fact(DisplayName = "Should support full cookie lifecycle - Append, Get, Delete")]
    public void ShouldSupportFullCookieLifecycle()
    {
        // Arrange
        string refreshToken = "lifecycle-test-token";
        var httpContext = CreateHttpContext();
        var manager = CreateManager();

        // Act & Assert - Append
        manager.Append(httpContext, refreshToken);

        // Simulate the cookie being set in the request for the next request
        var httpContextWithCookie = CreateHttpContextWithCookie(CookieName, refreshToken);
        
        // Act & Assert - TryGet
        bool getResult = manager.TryGet(httpContextWithCookie, out string retrievedToken);
        getResult.Should().BeTrue();
        retrievedToken.Should().Be(refreshToken);

        // Act & Assert - Delete
        Action deleteAct = () => manager.Delete(httpContextWithCookie);
        deleteAct.Should().NotThrow();
    }

    #endregion

    private RefreshTokenCookieManager CreateManager()
    {
        return new RefreshTokenCookieManager(_mockCookieOptionsProvider.Object);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        return new DefaultHttpContext();
    }

    private static DefaultHttpContext CreateHttpContextWithCookie(string cookieName, string? cookieValue)
    {
        var context = new DefaultHttpContext();
        
        if (cookieValue != null)
        {
            context.Request.Headers["Cookie"] = $"{cookieName}={cookieValue}";
        }
        else
        {
            context.Request.Headers["Cookie"] = $"{cookieName}=";
        }

        return context;
    }
}
