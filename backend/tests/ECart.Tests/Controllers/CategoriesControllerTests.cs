using ECart.Api.Controllers;
using ECart.Application.DTOs.Categories;
using ECart.Application.Interfaces.Services;
using ECart.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ECart.Tests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> _categoryServiceMock;
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _categoryServiceMock = new Mock<ICategoryService>();
        _controller = new CategoriesController(_categoryServiceMock.Object);
    }

    [Fact]
    public async Task GetAllCategories_ReturnsOkResultWithList()
    {
        // Arrange
        var categories = new List<CategoryDto>
        {
            new() { Id = Guid.NewGuid(), Name = "Electronics", Slug = "electronics", ProductCount = 5 },
            new() { Id = Guid.NewGuid(), Name = "Clothing", Slug = "clothing", ProductCount = 10 }
        };

        _categoryServiceMock
            .Setup(s => s.GetAllCategoriesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        // Act
        var result = await _controller.GetAllCategories(CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<IReadOnlyList<CategoryDto>>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal(2, apiResponse.Data?.Count);
    }

    [Fact]
    public async Task GetCategoryById_ReturnsOkResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var categoryDto = new CategoryDto
        {
            Id = categoryId,
            Name = "Books",
            Slug = "books",
            Description = "All books"
        };

        _categoryServiceMock
            .Setup(s => s.GetCategoryByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoryDto);

        // Act
        var result = await _controller.GetCategoryById(categoryId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<CategoryDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("Books", apiResponse.Data?.Name);
    }

    [Fact]
    public async Task CreateCategory_ValidModel_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateCategoryDto { Name = "Shoes", Description = "Running shoes" };
        var createdDto = new CategoryDto { Id = Guid.NewGuid(), Name = "Shoes", Slug = "shoes" };

        _categoryServiceMock
            .Setup(s => s.CreateCategoryAsync(createDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdDto);

        // Act
        var result = await _controller.CreateCategory(createDto, CancellationToken.None);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdAtActionResult.StatusCode);
        var apiResponse = Assert.IsType<ApiResponse<CategoryDto>>(createdAtActionResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("Shoes", apiResponse.Data?.Name);
    }

    [Fact]
    public async Task CreateCategory_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateCategoryDto();
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.CreateCategory(createDto, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(badRequestResult.Value);
        Assert.False(apiResponse.Success);
    }

    [Fact]
    public async Task UpdateCategory_ValidModel_ReturnsOk()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateCategoryDto { Name = "Updated Shoes" };
        var category = new CategoryDto { Id = id, Name = "Updated Shoes" };

        _categoryServiceMock
            .Setup(s => s.UpdateCategoryAsync(id, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // Act
        var result = await _controller.UpdateCategory(id, dto, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse<CategoryDto>>(okResult.Value);
        Assert.True(apiResponse.Success);
    }

    [Fact]
    public async Task UpdateCategory_InvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateCategoryDto();
        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.UpdateCategory(id, dto, CancellationToken.None);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(badRequestResult.Value);
        Assert.False(apiResponse.Success);
    }

    [Fact]
    public async Task DeleteCategory_ValidId_ReturnsOkResult()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _categoryServiceMock
            .Setup(s => s.DeleteCategoryAsync(categoryId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteCategory(categoryId, CancellationToken.None);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var apiResponse = Assert.IsType<ApiResponse>(okResult.Value);
        Assert.True(apiResponse.Success);
        Assert.Equal("Category deleted successfully.", apiResponse.Message);
    }
}
