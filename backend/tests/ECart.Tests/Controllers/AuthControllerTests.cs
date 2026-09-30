using System.Security.Claims;
using ECart.Api.Controllers;
using ECart.Application.DTOs.Auth;
using ECart.Application.Interfaces.Services;
using ECart.Domain.Common;
using ECart.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ECart.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly AuthController _controller;
    private readonly Guid _userId = Guid.NewGuid();

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _controller = new AuthController(_authServiceMock.Object);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _userId.ToString()),
            new Claim(ClaimTypes.Role, "Customer")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(identity) }
        };
    }

    [Fact]
    public async Task Register_ValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var request = new RegisterRequestDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            FirstName = "Test",
            LastName = "User"
        };
        var authResponse = new AuthResponseDto
        {
            Token = "jwt_token_123",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new UserDto
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Role = "Customer"
            }
        };

        _authServiceMock
            .Setup(s => s.RegisterAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Register(request, CancellationToken.None);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, objectResult.StatusCode);
        var apiResponse = Assert.IsType<ApiResponse<AuthResponseDto>>(objectResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("jwt_token_123", apiResponse.Data?.Token);
    }

    [Fact]
    public async Task Register_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequestDto();
        _controller.ModelState.AddModelError("Username", "Username is required");

        // Act
        var result = await _controller.Register(request, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(badRequestResult.Value);
        Assert.False(apiResponse.Success);
        Assert.Contains("Validation failed.", apiResponse.Message);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkResult()
    {
        // Arrange
        var request = new LoginRequestDto
        {
            UsernameOrEmail = "testuser",
            Password = "Password123!"
        };
        var authResponse = new AuthResponseDto
        {
            Token = "jwt_token_123",
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = new UserDto
            {
                Id = Guid.NewGuid(),
                Username = "testuser",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                Role = "Customer"
            }
        };

        _authServiceMock
            .Setup(s => s.LoginAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(authResponse);

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<AuthResponseDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("Authentication successful.", apiResponse.Message);
    }

    [Fact]
    public async Task Login_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequestDto();
        _controller.ModelState.AddModelError("Password", "Password is required");

        // Act
        var result = await _controller.Login(request, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(badRequestResult.Value);
        Assert.False(apiResponse.Success);
    }

    [Fact]
    public async Task GetCurrentUser_Authenticated_ReturnsOk()
    {
        // Arrange
        var userDto = new UserDto { Id = _userId, Username = "testuser" };
        _authServiceMock
            .Setup(s => s.GetCurrentUserAsync(_userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userDto);

        // Act
        var result = await _controller.GetCurrentUser(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<UserDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(_userId, apiResponse.Data?.Id);
    }

    [Fact]
    public async Task GetCurrentUser_Unauthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _controller.GetCurrentUser(CancellationToken.None));
    }

    [Fact]
    public async Task GetAllUsers_ReturnsOkResultWithUsers()
    {
        // Arrange
        var users = new List<UserDto>
        {
            new() { Id = Guid.NewGuid(), Username = "user1", Email = "user1@example.com", Role = "Customer" },
            new() { Id = Guid.NewGuid(), Username = "admin1", Email = "admin@example.com", Role = "Admin" }
        };

        _authServiceMock
            .Setup(s => s.GetAllUsersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        // Act
        var result = await _controller.GetAllUsers(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<IReadOnlyList<UserDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(2, apiResponse.Data?.Count);
    }
}
