using System.Security.Claims;
using ECart.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ECart.Tests.Controllers;

// Concrete implementation to test protected methods of BaseApiController
public class TestBaseApiController : BaseApiController
{
    public Guid? TestGetCurrentUserId() => GetCurrentUserId();
    public string? TestGetCurrentUserRole() => GetCurrentUserRole();
    public bool TestIsAdmin() => IsAdmin();
}

public class BaseApiControllerTests
{
    [Fact]
    public void GetCurrentUserId_ValidGuidClaim_ReturnsGuid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var controller = new TestBaseApiController();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        }, "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = controller.TestGetCurrentUserId();

        // Assert
        Assert.Equal(userId, result);
    }

    [Fact]
    public void GetCurrentUserId_InvalidOrMissingClaim_ReturnsNull()
    {
        // Arrange
        var controller = new TestBaseApiController();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "invalid-guid-string")
        }, "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = controller.TestGetCurrentUserId();

        // Assert
        Assert.Null(result);

        // Missing claim test
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        Assert.Null(controller.TestGetCurrentUserId());
    }

    [Fact]
    public void GetCurrentUserRole_ReturnsRole()
    {
        // Arrange
        var controller = new TestBaseApiController();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin")
        }, "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act & Assert
        Assert.Equal("Admin", controller.TestGetCurrentUserRole());
        Assert.True(controller.TestIsAdmin());
    }

    [Fact]
    public void IsAdmin_CustomerOrMissingRole_ReturnsFalse()
    {
        // Arrange
        var controller = new TestBaseApiController();
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Customer")
        }, "TestAuth"));

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act & Assert
        Assert.False(controller.TestIsAdmin());

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        Assert.False(controller.TestIsAdmin());
    }
}
