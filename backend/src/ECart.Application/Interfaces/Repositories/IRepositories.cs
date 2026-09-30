using ECart.Domain.Common;
using ECart.Domain.Entities;
using ECart.Application.DTOs.Products;

namespace ECart.Application.Interfaces.Repositories;

public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default);
}

public interface IProductRepository : IGenericRepository<Product>
{
    Task<Product?> GetWithCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<Product>> GetPagedProductsAsync(ProductQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<bool> SkuExistsAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default);
}

public interface ICategoryRepository : IGenericRepository<Category>
{
    Task<Category?> GetWithProductsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllWithProductCountsAsync(CancellationToken cancellationToken = default);
    Task<bool> SlugExistsAsync(string slug, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default);
    Task<bool> NameExistsAsync(string name, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default);
}

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<Order?> GetWithItemsByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Order>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
}

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    IOrderRepository Orders { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
