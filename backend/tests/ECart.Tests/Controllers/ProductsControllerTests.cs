using ECart.Api.Controllers;
using ECart.Application.DTOs.Products;
using ECart.Application.Interfaces.Services;
using ECart.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ECart.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _productServiceMock;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _productServiceMock = new Mock<IProductService>();
        _controller = new ProductsController(_productServiceMock.Object);
    }

    [Fact]
    public async Task GetProducts_ReturnsOkResult()
    {
        // Arrange
        var paged = new PagedResult<ProductDto>(new List<ProductDto>(), 0, 1, 10);
        _productServiceMock
            .Setup(s => s.GetProductsAsync(It.IsAny<ProductQueryParameters>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(paged);

        // Act
        var result = await _controller.GetProducts(new ProductQueryParameters(), CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<PagedResult<ProductDto>>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task GetProductById_ReturnsOkResult()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = new ProductDto { Id = id, Name = "Item A" };
        _productServiceMock
            .Setup(s => s.GetProductByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.GetProductById(id, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<ProductDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Item A", response.Data?.Name);
    }

    [Fact]
    public async Task CreateProduct_ValidDto_ReturnsCreated()
    {
        // Arrange
        var createDto = new CreateProductDto { Name = "Item B", Sku = "SKU-B" };
        var createdDto = new ProductDto { Id = Guid.NewGuid(), Name = "Item B" };
        _productServiceMock
            .Setup(s => s.CreateProductAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        // Act
        var result = await _controller.CreateProduct(createDto, CancellationToken.None);

        // Assert
        var createdResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
    }

    [Fact]
    public async Task CreateProduct_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateProductDto();
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.CreateProduct(createDto, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(badRequestResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task UpdateProduct_ValidDto_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updateDto = new UpdateProductDto { Name = "Updated Item" };
        var updatedDto = new ProductDto { Id = id, Name = "Updated Item" };
        _productServiceMock
            .Setup(s => s.UpdateProductAsync(id, updateDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedDto);

        // Act
        var result = await _controller.UpdateProduct(id, updateDto, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<ProductDto>>(okResult.Value);
        Assert.True(response.Success);
    }

    [Fact]
    public async Task UpdateProduct_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updateDto = new UpdateProductDto();
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.UpdateProduct(id, updateDto, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(badRequestResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task DeleteProduct_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        _productServiceMock
            .Setup(s => s.DeleteProductAsync(id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteProduct(id, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(response.Success);
    }
}
