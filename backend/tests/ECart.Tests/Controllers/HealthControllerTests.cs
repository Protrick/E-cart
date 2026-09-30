using ECart.Api.Controllers;
using ECart.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ECart.Tests.Controllers;

public class HealthControllerTests
{
    [Fact]
    public void CheckHealth_ReturnsOkWithHealthyStatus()
    {
        // Arrange
        var controller = new HealthController();

        // Act
        var result = controller.CheckHealth();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<object>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Contains("operational", apiResponse.Message);
    }
}
