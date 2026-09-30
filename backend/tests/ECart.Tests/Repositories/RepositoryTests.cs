using ECart.Application.DTOs.Products;
using ECart.Domain.Entities;
using ECart.Domain.Enums;
using ECart.Infrastructure.Data;
using ECart.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ECart.Tests.Repositories;

public class RepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UnitOfWork _unitOfWork;

    public RepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
    }

    public void Dispose()
    {
        _unitOfWork.Dispose();
    }

    [Fact]
    public async Task UserRepository_GetByEmailAndUsername_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "TestUser",
            Email = "Test@example.com",
            PasswordHash = "hash",
            Role = UserRole.Customer,
            IsActive = true
        };
        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var byEmail = await _unitOfWork.Users.GetByEmailAsync("test@example.com");
        var byUsername = await _unitOfWork.Users.GetByUsernameAsync("testuser");
        var byEmailOrUser = await _unitOfWork.Users.GetByUsernameOrEmailAsync("test@example.com");

        // Assert
        Assert.NotNull(byEmail);
        Assert.NotNull(byUsername);
        Assert.NotNull(byEmailOrUser);
        Assert.Equal(user.Id, byEmail.Id);
    }

    [Fact]
    public async Task CategoryRepository_CRUD_And_SlugCheck()
    {
        // Arrange
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Gadgets",
            Slug = "gadgets",
            Description = "All gadgets",
            IsActive = true
        };
        await _unitOfWork.Categories.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var slugExists = await _unitOfWork.Categories.SlugExistsAsync("gadgets", null);
        var slugExcluded = await _unitOfWork.Categories.SlugExistsAsync("gadgets", category.Id);
        var nameExists = await _unitOfWork.Categories.NameExistsAsync("gadgets", null);
        var nameExcluded = await _unitOfWork.Categories.NameExistsAsync("gadgets", category.Id);
        var all = await _unitOfWork.Categories.GetAllWithProductCountsAsync();
        var byId = await _unitOfWork.Categories.GetWithProductsByIdAsync(category.Id);

        // Assert
        Assert.True(slugExists);
        Assert.False(slugExcluded);
        Assert.True(nameExists);
        Assert.False(nameExcluded);
        Assert.Single(all);
        Assert.NotNull(byId);
    }

    [Fact]
    public async Task ProductRepository_PagedAndFilters_WorkCorrectly()
    {
        // Arrange
        var category = new Category { Id = Guid.NewGuid(), Name = "Electronics", Slug = "electronics" };
        await _unitOfWork.Categories.AddAsync(category);

        var p1 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Smartphone X",
            Description = "Best phone ever",
            Sku = "PHONE-X",
            Price = 699.99m,
            StockQuantity = 10,
            CategoryId = category.Id,
            IsActive = true
        };
        var p2 = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Laptop Pro",
            Description = "Best laptop",
            Sku = "LAP-PRO",
            Price = 1299.99m,
            StockQuantity = 0,
            CategoryId = category.Id,
            IsActive = true
        };

        await _unitOfWork.Products.AddAsync(p1);
        await _unitOfWork.Products.AddAsync(p2);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var skuExists = await _unitOfWork.Products.SkuExistsAsync("PHONE-X", null);
        var skuExcluded = await _unitOfWork.Products.SkuExistsAsync("PHONE-X", p1.Id);

        // Test filter by search and in stock
        var pagedAll = await _unitOfWork.Products.GetPagedProductsAsync(new ProductQueryParameters
        {
            Search = "phone",
            SortBy = "price",
            IsDescending = true,
            InStockOnly = true,
            MinPrice = 100,
            MaxPrice = 1000,
            CategoryId = category.Id,
            PageNumber = 0, // tests pageNumber < 1 branch
            PageSize = 0    // tests pageSize < 1 branch
        });

        // Test sorting branches
        var sortName = await _unitOfWork.Products.GetPagedProductsAsync(new ProductQueryParameters { SortBy = "name", IsDescending = false });
        var sortNameDesc = await _unitOfWork.Products.GetPagedProductsAsync(new ProductQueryParameters { SortBy = "name", IsDescending = true });
        var sortStock = await _unitOfWork.Products.GetPagedProductsAsync(new ProductQueryParameters { SortBy = "stock", IsDescending = false });
        var sortStockDesc = await _unitOfWork.Products.GetPagedProductsAsync(new ProductQueryParameters { SortBy = "stock", IsDescending = true });
        var sortDefaultDesc = await _unitOfWork.Products.GetPagedProductsAsync(new ProductQueryParameters { SortBy = "unknown", IsDescending = true });

        var byId = await _unitOfWork.Products.GetWithCategoryByIdAsync(p1.Id);

        // Assert
        Assert.True(skuExists);
        Assert.False(skuExcluded);
        Assert.Single(pagedAll.Items);
        Assert.Equal("Smartphone X", pagedAll.Items.First().Name);
        Assert.NotNull(byId);
        Assert.Equal(2, sortName.Items.Count());
        Assert.Equal(2, sortNameDesc.Items.Count());
        Assert.Equal(2, sortStock.Items.Count());
        Assert.Equal(2, sortStockDesc.Items.Count());
        Assert.Equal(2, sortDefaultDesc.Items.Count());
    }

    [Fact]
    public async Task OrderRepository_CRUD_WorksCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Username = "shopper", Email = "shopper@test.com", PasswordHash = "hash" };
        await _unitOfWork.Users.AddAsync(user);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ShippingAddress = "456 Oak St",
            Status = OrderStatus.Pending,
            TotalAmount = 250m
        };
        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.SaveChangesAsync();

        // Act
        var userOrders = await _unitOfWork.Orders.GetByUserIdAsync(userId);
        var orderById = await _unitOfWork.Orders.GetWithItemsByIdAsync(order.Id);
        var allOrders = await _unitOfWork.Orders.GetAllWithDetailsAsync();

        // Assert
        Assert.Single(userOrders);
        Assert.NotNull(orderById);
        Assert.Single(allOrders);
    }

    [Fact]
    public async Task GenericRepository_AllMethods_ExecuteCorrectly()
    {
        // Arrange
        var genericRepo = new GenericRepository<User>(_context);
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "genericuser",
            Email = "generic@test.com",
            PasswordHash = "h",
            Role = UserRole.Customer
        };

        // Add
        await genericRepo.AddAsync(user);
        await _context.SaveChangesAsync();

        // GetById
        var fetched = await genericRepo.GetByIdAsync(user.Id);
        Assert.NotNull(fetched);

        // GetAll
        var all = await genericRepo.GetAllAsync();
        Assert.NotEmpty(all);

        // Find
        var found = await genericRepo.FindAsync(u => u.Username == "genericuser");
        Assert.Single(found);

        // Exists
        var exists = await genericRepo.ExistsAsync(u => u.Username == "genericuser");
        Assert.True(exists);

        // Count with predicate and without predicate
        var countAll = await genericRepo.CountAsync();
        var countFiltered = await genericRepo.CountAsync(u => u.Email == "generic@test.com");
        Assert.True(countAll >= 1);
        Assert.Equal(1, countFiltered);

        // Update
        user.FirstName = "Updated";
        await genericRepo.UpdateAsync(user);
        await _context.SaveChangesAsync();

        // Delete
        await genericRepo.DeleteAsync(user);
        await _context.SaveChangesAsync();

        var existsAfterDelete = await genericRepo.ExistsAsync(u => u.Id == user.Id);
        Assert.False(existsAfterDelete);
    }
}
