using ECart.Application.DTOs.Categories;
using ECart.Application.Interfaces.Repositories;
using ECart.Application.Services;
using ECart.Domain.Entities;
using ECart.Domain.Exceptions;
using Moq;
using Xunit;

namespace ECart.Tests.Services;

public class CategoryServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICategoryRepository> _categoryRepoMock;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _categoryRepoMock = new Mock<ICategoryRepository>();

        _unitOfWorkMock.Setup(u => u.Categories).Returns(_categoryRepoMock.Object);

        _service = new CategoryService(_unitOfWorkMock.Object);
    }

    [Fact]
    public async Task GetAllCategoriesAsync_ReturnsMappedCategories()
    {
        // Arrange
        var categories = new List<Category>
        {
            new() { Id = Guid.NewGuid(), Name = "Electronics", Slug = "electronics", Products = new List<Product>() },
            new() { Id = Guid.NewGuid(), Name = "Books", Slug = "books", Products = null }
        };

        _categoryRepoMock
            .Setup(r => r.GetAllWithProductCountsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _service.GetAllCategoriesAsync(CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("Electronics", result[0].Name);
        Assert.Equal(0, result[1].ProductCount);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ExistingCategory_ReturnsDto()
    {
        // Arrange
        var id = Guid.NewGuid();
        var category = new Category { Id = id, Name = "Fashion", Slug = "fashion", Products = new List<Product>() };

        _categoryRepoMock
            .Setup(r => r.GetWithProductsByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _service.GetCategoryByIdAsync(id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Fashion", result.Name);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _categoryRepoMock
            .Setup(r => r.GetWithProductsByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetCategoryByIdAsync(id, CancellationToken.None));
    }

    [Fact]
    public async Task CreateCategoryAsync_EmptySlug_GeneratesSlugFromSpecialChars()
    {
        // Arrange
        var dto = new CreateCategoryDto { Name = "Home & Kitchen / Tools \\ Hardware", Slug = null };

        _categoryRepoMock
            .Setup(r => r.NameExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _categoryRepoMock
            .Setup(r => r.SlugExistsAsync(It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreateCategoryAsync(dto, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("home-and-kitchen---tools---hardware", result.Slug);
        _categoryRepoMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCategoryAsync_DuplicateName_ThrowsConflictException()
    {
        // Arrange
        var dto = new CreateCategoryDto { Name = "Toys", Slug = "toys" };

        _categoryRepoMock
            .Setup(r => r.NameExistsAsync("Toys", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateCategoryAsync(dto, CancellationToken.None));
    }

    [Fact]
    public async Task CreateCategoryAsync_DuplicateSlug_ThrowsConflictException()
    {
        // Arrange
        var dto = new CreateCategoryDto { Name = "Toys", Slug = "toys" };

        _categoryRepoMock
            .Setup(r => r.NameExistsAsync("Toys", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _categoryRepoMock
            .Setup(r => r.SlugExistsAsync("toys", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.CreateCategoryAsync(dto, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateCategoryAsync_Valid_UpdatesCategory()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new Category { Id = id, Name = "Old", Slug = "old" };
        var dto = new UpdateCategoryDto { Name = "New Name", Description = "Desc", IsActive = true, Slug = "new-slug" };

        _categoryRepoMock
            .Setup(r => r.GetWithProductsByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _categoryRepoMock
            .Setup(r => r.NameExistsAsync("New Name", id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _categoryRepoMock
            .Setup(r => r.SlugExistsAsync("new-slug", id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.UpdateCategoryAsync(id, dto, CancellationToken.None);

        // Assert
        Assert.Equal("New Name", result.Name);
        _categoryRepoMock.Verify(r => r.UpdateAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCategoryAsync_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _categoryRepoMock.Setup(r => r.GetWithProductsByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateCategoryAsync(id, new UpdateCategoryDto(), CancellationToken.None));
    }

    [Fact]
    public async Task UpdateCategoryAsync_DuplicateName_ThrowsConflictException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new Category { Id = id, Name = "Old" };
        _categoryRepoMock.Setup(r => r.GetWithProductsByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _categoryRepoMock.Setup(r => r.NameExistsAsync("New", id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateCategoryAsync(id, new UpdateCategoryDto { Name = "New" }, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateCategoryAsync_DuplicateSlug_ThrowsConflictException()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new Category { Id = id, Name = "Old" };
        _categoryRepoMock.Setup(r => r.GetWithProductsByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        _categoryRepoMock.Setup(r => r.NameExistsAsync("New", id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _categoryRepoMock.Setup(r => r.SlugExistsAsync("slug-x", id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _service.UpdateCategoryAsync(id, new UpdateCategoryDto { Name = "New", Slug = "slug-x" }, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteCategoryAsync_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        var id = Guid.NewGuid();
        _categoryRepoMock.Setup(r => r.GetWithProductsByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteCategoryAsync(id, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteCategoryAsync_WithActiveProducts_ThrowsBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new Category
        {
            Id = id,
            Products = new List<Product> { new() { IsActive = true } }
        };

        _categoryRepoMock
            .Setup(r => r.GetWithProductsByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _service.DeleteCategoryAsync(id, CancellationToken.None));
    }

    [Fact]
    public async Task DeleteCategoryAsync_WithoutActiveProducts_DeletesSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existing = new Category { Id = id, Products = new List<Product>() };

        _categoryRepoMock
            .Setup(r => r.GetWithProductsByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        await _service.DeleteCategoryAsync(id, CancellationToken.None);

        // Assert
        _categoryRepoMock.Verify(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }
}
