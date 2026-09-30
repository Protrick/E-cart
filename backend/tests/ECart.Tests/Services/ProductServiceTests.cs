using ECart.Application.DTOs.Products;
using ECart.Application.Interfaces.Repositories;
using ECart.Application.Services;
using ECart.Domain.Common;
using ECart.Domain.Entities;
using ECart.Domain.Exceptions;
using Moq;
using Xunit;

namespace ECart.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<ICategoryRepository> _categoryRepoMock;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _productRepoMock = new Mock<IProductRepository>();
        _categoryRepoMock = new Mock<ICategoryRepository>();

        _unitOfWorkMock.Setup(u => u.Products).Returns(_productRepoMock.Object);
        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepoMock.Object);

        _service = new ProductService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetProductsAsync_ReturnsPagedResults()
    {
        // Arrange
        var parameters = new ProductQueryParameters();
        var pagedEntities = new PagedResult<Product>(
            new List<Product>
            {
                new() { Id = Guid.NewGuid(), Name = "P1", Sku = "SKU1", Price = 10, Category = null }
            },
            1, 1, 10);

        _productRepoMock
            .Setup(r => r.GetPagedProductsAsync(parameters, It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedEntities);

        // Act
        var result = await _service.GetProductsAsync(parameters, CancellationToken.None);

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("P1", result.Items.First().Name);
        Assert.Equal(string.Empty, result.Items.First().CategoryName);
    }

    [Fact]
    public async Task GetProductByIdAsync_ExistingProduct_ReturnsProductDto()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product
        {
            Id = productId,
            Name = "Mechanical Keyboard",
            Description = "RGB Gaming Keyboard",
            Sku = "KEY-001",
            Price = 79.99m,
            StockQuantity = 20,
            Category = new Category { Id = Guid.NewGuid(), Name = "Accessories" }
        };

        _productRepoMock
            .Setup(r => r.GetWithCategoryByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        var result = await _service.GetProductByIdAsync(productId, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Mechanical Keyboard", result.Name);
        Assert.Equal("Accessories", result.CategoryName);
    }

    [Fact]
    public async Task GetProductByIdAsync_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _productRepoMock
            .Setup(r => r.GetWithCategoryByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetProductByIdAsync(productId, CancellationToken.None));
    }

    [Fact]
    public async Task CreateProductAsync_ValidProduct_ReturnsCreatedProductDto()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Electronics" };
        var createDto = new CreateProductDto
        {
            Name = "Smartphone",
            Description = "Latest model",
            Sku = "PHN-101",
            Price = 999.99m,
            StockQuantity = 15,
            CategoryId = categoryId,
            ImageUrl = "https://example.com/img.jpg"
        };

        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _productRepoMock
            .Setup(r => r.SkuExistsAsync("PHN-101", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreateProductAsync(createDto, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Smartphone", result.Name);
        Assert.Equal("PHN-101", result.Sku);
        _productRepoMock.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_CategoryNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryRepoMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        var createDto = new CreateProductDto { CategoryId = categoryId, Sku = "SKU-99" };

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateProductAsync(createDto, CancellationToken.None));
    }

    [Fact]
    public async Task CreateProductAsync_DuplicateSku_ThrowsConflictException()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Electronics" };
        var createDto = new CreateProductDto
        {
            Name = "Smartphone",
            Description = "Latest model",
            Sku = "PHN-101",
            Price = 999.99m,
            StockQuantity = 15,
            CategoryId = categoryId
        };

        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _productRepoMock
            .Setup(r => r.SkuExistsAsync("PHN-101", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateProductAsync(createDto, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateProductAsync_Valid_UpdatesAndReturnsDto()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var category = new Category { Id = categoryId, Name = "Electronics" };
        var existing = new Product { Id = productId, Name = "Old Name", Sku = "SKU-OLD", CategoryId = categoryId };
        var updateDto = new UpdateProductDto
        {
            Name = "New Name",
            Description = "Updated Desc",
            Sku = "SKU-NEW",
            Price = 50m,
            StockQuantity = 10,
            CategoryId = categoryId,
            IsActive = true,
            ImageUrl = "https://example.com/updated.jpg"
        };

        _productRepoMock.Setup(r => r.GetWithCategoryByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _categoryRepoMock.Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _productRepoMock.Setup(r => r.SkuExistsAsync("SKU-NEW", productId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.UpdateProductAsync(productId, updateDto, CancellationToken.None);

        // Assert
        Assert.Equal("New Name", result.Name);
        _productRepoMock.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_ProductNotFound_ThrowsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _productRepoMock.Setup(r => r.GetWithCategoryByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateProductAsync(id, new UpdateProductDto(), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateProductAsync_CategoryNotFound_ThrowsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var catId = Guid.NewGuid();
        var existing = new Product { Id = id };
        _productRepoMock.Setup(r => r.GetWithCategoryByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _categoryRepoMock.Setup(r => r.GetByIdAsync(catId, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateProductAsync(id, new UpdateProductDto { CategoryId = catId }, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateProductAsync_DuplicateSku_ThrowsConflict()
    {
        // Arrange
        var id = Guid.NewGuid();
        var catId = Guid.NewGuid();
        var existing = new Product { Id = id };
        var category = new Category { Id = catId };
        _productRepoMock.Setup(r => r.GetWithCategoryByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _categoryRepoMock.Setup(r => r.GetByIdAsync(catId, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _productRepoMock.Setup(r => r.SkuExistsAsync("DUP-SKU", id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateProductAsync(id, new UpdateProductDto { CategoryId = catId, Sku = "DUP-SKU" }, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteProductAsync_ExistingProduct_DeletesSuccessfully()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var product = new Product { Id = productId, Name = "To Delete" };

        _productRepoMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        // Act
        await _service.DeleteProductAsync(productId, CancellationToken.None);

        // Assert
        _productRepoMock.Verify(r => r.DeleteAsync(product, It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_ProductNotFound_ThrowsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _productRepoMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteProductAsync(id, CancellationToken.None));
    }
}
