using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using SyncChat.API.Infrastructure.Security;
using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;

namespace SyncChat.Test.Infrastructure.Security;

[ExcludeFromCodeCoverage(Justification = "This is a test class and does not require coverage.")]
public class IdentityServiceTests
{
    private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

    public IdentityServiceTests()
    {
        _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
    }

    #region GetUserID Tests

    [Fact(DisplayName = "GetUserID should return user ID when valid claim exists")]
    public void GetUserID_ShouldReturnUserId_WhenValidClaimExists()
    {
        // Arrange
        long expectedUserId = 12345;
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new IdentityService(_mockHttpContextAccessor.Object);

        // Act
        long actualUserId = service.GetUserID();

        // Assert
        actualUserId.Should().Be(expectedUserId);
    }

    [Fact(DisplayName = "GetUserID should return 0 when claim does not exist")]
    public void GetUserID_ShouldReturnZero_WhenClaimDoesNotExist()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new IdentityService(_mockHttpContextAccessor.Object);

        // Act
        long userId = service.GetUserID();

        // Assert
        userId.Should().Be(0);
    }

    [Fact(DisplayName = "GetUserID should return 0 when claim value is not a valid long")]
    public void GetUserID_ShouldReturnZero_WhenClaimValueIsNotValidLong()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "not-a-number")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new IdentityService(_mockHttpContextAccessor.Object);

        // Act
        long userId = service.GetUserID();

        // Assert
        userId.Should().Be(0);
    }

    [Fact(DisplayName = "GetUserID should return 0 when claim value is empty")]
    public void GetUserID_ShouldReturnZero_WhenClaimValueIsEmpty()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, string.Empty)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new IdentityService(_mockHttpContextAccessor.Object);

        // Act
        long userId = service.GetUserID();

        // Assert
        userId.Should().Be(0);
    }

    [Fact(DisplayName = "GetUserID should return 0 when user has no claims")]
    public void GetUserID_ShouldReturnZero_WhenUserHasNoClaims()
    {
        // Arrange
        var identity = new ClaimsIdentity();
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new IdentityService(_mockHttpContextAccessor.Object);

        // Act
        long userId = service.GetUserID();

        // Assert
        userId.Should().Be(0);
    }

    [Fact(DisplayName = "GetUserID should throw InvalidOperationException when HttpContext is null")]
    public void GetUserID_ShouldThrowInvalidOperationException_WhenHttpContextIsNull()
    {
        // Arrange
        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns((HttpContext)null!);

        var service = new IdentityService(_mockHttpContextAccessor.Object);

        // Act
        Action act = () => service.GetUserID();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No HttpContext.");
    }

    [Theory(DisplayName = "GetUserID should handle various valid user ID formats")]
    [InlineData("1", 1)]
    [InlineData("123", 123)]
    [InlineData("999999", 999999)]
    [InlineData("9223372036854775807", 9223372036854775807)] // long.MaxValue
    public void GetUserID_ShouldHandleVariousValidUserIdFormats(string claimValue, long expectedUserId)
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, claimValue)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new IdentityService(_mockHttpContextAccessor.Object);

        // Act
        long actualUserId = service.GetUserID();

        // Assert
        actualUserId.Should().Be(expectedUserId);
    }

    [Theory(DisplayName = "GetUserID should return 0 for invalid formats")]
    [InlineData("abc")]
    [InlineData("12.34")]
    [InlineData("12-34")]
    [InlineData("0x123")]
    [InlineData("   ")]
    public void GetUserID_ShouldReturnZero_ForInvalidFormats(string claimValue)
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, claimValue)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new IdentityService(_mockHttpContextAccessor.Object);

        // Act
        long userId = service.GetUserID();

        // Assert
        userId.Should().Be(0);
    }

    [Fact(DisplayName = "GetUserID should use first NameIdentifier claim when multiple exist")]
    public void GetUserID_ShouldUseFirstNameIdentifierClaim_WhenMultipleExist()
    {
        // Arrange
        long expectedUserId = 111;
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString()),
            new Claim(ClaimTypes.NameIdentifier, "222"),
            new Claim(ClaimTypes.NameIdentifier, "333")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new IdentityService(_mockHttpContextAccessor.Object);

        // Act
        long actualUserId = service.GetUserID();

        // Assert
        actualUserId.Should().Be(expectedUserId);
    }

    [Fact(DisplayName = "GetUserID should work with authenticated user")]
    public void GetUserID_ShouldWork_WithAuthenticatedUser()
    {
        // Arrange
        long expectedUserId = 42;
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString()),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim(ClaimTypes.Name, "John Doe")
        };
        var identity = new ClaimsIdentity(claims, "Bearer");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new IdentityService(_mockHttpContextAccessor.Object);

        // Act
        long actualUserId = service.GetUserID();

        // Assert
        actualUserId.Should().Be(expectedUserId);
        claimsPrincipal.Identity?.IsAuthenticated.Should().BeTrue();
    }

    [Fact(DisplayName = "GetUserID should return 0 for unauthenticated user with no claims")]
    public void GetUserID_ShouldReturnZero_ForUnauthenticatedUserWithNoClaims()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // Not authenticated
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new IdentityService(_mockHttpContextAccessor.Object);

        // Act
        long userId = service.GetUserID();

        // Assert
        userId.Should().Be(0);
        claimsPrincipal.Identity?.IsAuthenticated.Should().BeFalse();
    }

    [Fact(DisplayName = "GetUserID should handle overflow values gracefully")]
    public void GetUserID_ShouldHandleOverflowValuesGracefully()
    {
        // Arrange - Value larger than long.MaxValue
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "99999999999999999999999999")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new IdentityService(_mockHttpContextAccessor.Object);

        // Act
        long userId = service.GetUserID();

        // Assert
        userId.Should().Be(0);
    }

    [Fact(DisplayName = "GetUserID should be callable multiple times with same result")]
    public void GetUserID_ShouldBeCallableMultipleTimes_WithSameResult()
    {
        // Arrange
        long expectedUserId = 777;
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, expectedUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);
        var httpContext = new DefaultHttpContext { User = claimsPrincipal };

        _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);

        var service = new IdentityService(_mockHttpContextAccessor.Object);

        // Act
        long userId1 = service.GetUserID();
        long userId2 = service.GetUserID();
        long userId3 = service.GetUserID();

        // Assert
        userId1.Should().Be(expectedUserId);
        userId2.Should().Be(expectedUserId);
        userId3.Should().Be(expectedUserId);
    }

    #endregion
}
