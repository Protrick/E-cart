using ECart.Application.DTOs.Orders;
using ECart.Application.Interfaces.Repositories;
using ECart.Application.Services;
using ECart.Domain.Entities;
using ECart.Domain.Enums;
using ECart.Domain.Exceptions;
using Moq;
using Xunit;

namespace ECart.Tests.Services;

public class OrderServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IOrderRepository> _orderRepoMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly OrderService _service;

    public OrderServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _orderRepoMock = new Mock<IOrderRepository>();
        _userRepoMock = new Mock<IUserRepository>();
        _productRepoMock = new Mock<IProductRepository>();

        _unitOfWorkMock.Setup(u => u.Orders).Returns(_orderRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Users).Returns(_userRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);

        _service = new OrderService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateOrderAsync_ValidRequest_CreatesOrderAndDeductsStock()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "buyer@test.com", FirstName = "John", LastName = "Doe", IsActive = true };
        var product = new Product { Id = productId, Name = "Laptop", Price = 1000m, StockQuantity = 5, IsActive = true, Sku = "LAP-1" };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _productRepoMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var dto = new CreateOrderDto
        {
            ShippingAddress = "123 Main St",
            Items = new List<CreateOrderItemDto>
            {
                new() { ProductId = productId, Quantity = 2 }
            }
        };

        // Act
        var result = await _service.CreateOrderAsync(userId, dto, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2000m, result.TotalAmount);
        Assert.Equal(3, product.StockQuantity); // 5 - 2 = 3
        Assert.Equal("John Doe", result.UserName);
        _orderRepoMock.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_InactiveUser_ThrowsNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        var dto = new CreateOrderDto { ShippingAddress = "123 Main St", Items = new() };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateOrderAsync(userId, dto, CancellationToken.None));
    }

    [Fact]
    public async Task CreateOrderAsync_EmptyItems_ThrowsBadRequestException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, IsActive = true };
        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var dto = new CreateOrderDto { ShippingAddress = "123 St", Items = new List<CreateOrderItemDto>() };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateOrderAsync(userId, dto, CancellationToken.None));
    }

    [Fact]
    public async Task CreateOrderAsync_ProductNotFoundOrInactive_ThrowsBadRequestException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var user = new User { Id = userId, IsActive = true };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _productRepoMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        var dto = new CreateOrderDto
        {
            ShippingAddress = "123 St",
            Items = new List<CreateOrderItemDto> { new() { ProductId = productId, Quantity = 1 } }
        };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateOrderAsync(userId, dto, CancellationToken.None));
    }

    [Fact]
    public async Task CreateOrderAsync_InsufficientStock_ThrowsBadRequestException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var user = new User { Id = userId, IsActive = true };
        var product = new Product { Id = productId, Name = "Phone", Price = 500m, StockQuantity = 1, IsActive = true };

        _userRepoMock.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _productRepoMock.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var dto = new CreateOrderDto
        {
            ShippingAddress = "123 Main St",
            Items = new List<CreateOrderItemDto> { new() { ProductId = productId, Quantity = 5 } }
        };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _service.CreateOrderAsync(userId, dto, CancellationToken.None));
    }

    [Fact]
    public async Task GetOrderByIdAsync_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderRepoMock.Setup(r => r.GetWithItemsByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetOrderByIdAsync(orderId, Guid.NewGuid(), false, CancellationToken.None));
    }

    [Fact]
    public async Task GetOrderByIdAsync_UnauthorizedUser_ThrowsForbiddenException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();
        var order = new Order { Id = orderId, UserId = ownerId };

        _orderRepoMock.Setup(r => r.GetWithItemsByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => _service.GetOrderByIdAsync(orderId, requestingUserId, false, CancellationToken.None));
    }

    [Fact]
    public async Task GetOrderByIdAsync_AdminViewingOtherUserOrder_ReturnsDto()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var ownerId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var order = new Order { Id = orderId, UserId = ownerId, OrderItems = new List<OrderItem>() };

        _orderRepoMock.Setup(r => r.GetWithItemsByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        // Act
        var result = await _service.GetOrderByIdAsync(orderId, adminId, true, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_OrderNotFound_ThrowsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderRepoMock.Setup(r => r.GetWithItemsByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync((Order?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateOrderStatusAsync(orderId, new UpdateOrderStatusDto(), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_CancelOrder_RestocksItems()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var product = new Product { Id = Guid.NewGuid(), StockQuantity = 10 };
        var order = new Order
        {
            Id = orderId,
            Status = OrderStatus.Processing,
            OrderItems = new List<OrderItem>
            {
                new() { Product = product, Quantity = 2 },
                new() { Product = null, Quantity = 1 }
            }
        };

        _orderRepoMock.Setup(r => r.GetWithItemsByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);

        var updateDto = new UpdateOrderStatusDto { Status = OrderStatus.Cancelled };

        // Act
        var result = await _service.UpdateOrderStatusAsync(orderId, updateDto, CancellationToken.None);

        // Assert
        Assert.Equal(OrderStatus.Cancelled, result.Status);
        Assert.Equal(12, product.StockQuantity); // 10 + 2 restocked
    }

    [Fact]
    public async Task GetMyOrdersAsync_ReturnsOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orders = new List<Order> { new() { Id = Guid.NewGuid(), UserId = userId, OrderItems = new List<OrderItem>() } };

        _orderRepoMock.Setup(r => r.GetByUserIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(orders);

        // Act
        var result = await _service.GetMyOrdersAsync(userId, CancellationToken.None);

        // Assert
        Assert.Single(result);
    }

    [Fact]
    public async Task GetAllOrdersAsync_ReturnsAllOrders()
    {
        // Arrange
        var orders = new List<Order> { new() { Id = Guid.NewGuid(), OrderItems = new List<OrderItem>() } };
        _orderRepoMock.Setup(r => r.GetAllWithDetailsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(orders);

        // Act
        var result = await _service.GetAllOrdersAsync(CancellationToken.None);

        // Assert
        Assert.Single(result);
    }
}
