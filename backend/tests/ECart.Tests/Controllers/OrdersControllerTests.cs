using System.Security.Claims;
using ECart.Api.Controllers;
using ECart.Application.DTOs.Orders;
using ECart.Application.Interfaces.Services;
using ECart.Domain.Common;
using ECart.Domain.Enums;
using ECart.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ECart.Tests.Controllers;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _orderServiceMock;
    private readonly OrdersController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();

    public OrdersControllerTests()
    {
        _orderServiceMock = new Mock<IOrderService>();
        _controller = new OrdersController(_orderServiceMock.Object);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString()),
            new Claim(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task CreateOrder_ValidDto_ReturnsCreated()
    {
        // Arrange
        var dto = new CreateOrderDto { ShippingAddress = "123 Main St", Items = new() };
        var orderDto = new OrderDto { Id = Guid.NewGuid(), TotalAmount = 100 };

        _orderServiceMock
            .Setup(s => s.CreateOrderAsync(_testUserId, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _controller.CreateOrder(dto, CancellationToken.None);

        // Assert
        var objResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status201Created, objResult.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var dto = new CreateOrderDto();
        _controller.ModelState.AddModelError("ShippingAddress", "Required");

        // Act
        var result = await _controller.CreateOrder(dto, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(badRequestResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task CreateOrder_Unauthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _controller.CreateOrder(new CreateOrderDto(), CancellationToken.None));
    }

    [Fact]
    public async Task GetMyOrders_ReturnsOk()
    {
        // Arrange
        var orders = new List<OrderDto> { new() { Id = Guid.NewGuid() } };
        _orderServiceMock
            .Setup(s => s.GetMyOrdersAsync(_testUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _controller.GetMyOrders(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IReadOnlyList<OrderDto>>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetMyOrders_Unauthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _controller.GetMyOrders(CancellationToken.None));
    }

    [Fact]
    public async Task GetAllOrders_ReturnsOk()
    {
        // Arrange
        var orders = new List<OrderDto> { new() { Id = Guid.NewGuid() } };
        _orderServiceMock
            .Setup(s => s.GetAllOrdersAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _controller.GetAllOrders(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IReadOnlyList<OrderDto>>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetOrderById_ReturnsOk()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderDto { Id = orderId };
        _orderServiceMock
            .Setup(s => s.GetOrderByIdAsync(orderId, _testUserId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _controller.GetOrderById(orderId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<OrderDto>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetOrderById_Unauthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => _controller.GetOrderById(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateOrderStatus_Valid_ReturnsOk()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var updateDto = new UpdateOrderStatusDto { Status = OrderStatus.Delivered };
        var orderDto = new OrderDto { Id = orderId, Status = OrderStatus.Delivered };

        _orderServiceMock
            .Setup(s => s.UpdateOrderStatusAsync(orderId, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orderDto);

        // Act
        var result = await _controller.UpdateOrderStatus(orderId, updateDto, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<OrderDto>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task UpdateOrderStatus_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var updateDto = new UpdateOrderStatusDto();
        _controller.ModelState.AddModelError("Status", "Required");

        // Act
        var result = await _controller.UpdateOrderStatus(orderId, updateDto, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(badRequestResult.Value);
        Assert.False(response.Success);
    }
}
