using ECart.Application.DTOs.Products;
using ECart.Application.Interfaces.Repositories;
using ECart.Domain.Common;
using ECart.Domain.Entities;
using ECart.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECart.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), cancellationToken);
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower(), cancellationToken);
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
    {
        var normalized = usernameOrEmail.ToLower();
        return await _dbSet.FirstOrDefaultAsync(u => u.Username.ToLower() == normalized || u.Email.ToLower() == normalized, cancellationToken);
    }
}

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetWithCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(p => p.Category)
                           .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Product>> GetPagedProductsAsync(ProductQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Include(p => p.Category).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameters.Search))
        {
            var searchTerm = parameters.Search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(searchTerm) || 
                                     p.Description.ToLower().Contains(searchTerm) || 
                                     p.Sku.ToLower().Contains(searchTerm));
        }

        if (parameters.CategoryId.HasValue && parameters.CategoryId.Value != Guid.Empty)
        {
            query = query.Where(p => p.CategoryId == parameters.CategoryId.Value);
        }

        if (parameters.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= parameters.MinPrice.Value);
        }

        if (parameters.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= parameters.MaxPrice.Value);
        }

        if (parameters.InStockOnly.HasValue && parameters.InStockOnly.Value)
        {
            query = query.Where(p => p.StockQuantity > 0);
        }

        // Sorting
        query = parameters.SortBy?.ToLower() switch
        {
            "name" => parameters.IsDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "price" => parameters.IsDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
            "stock" => parameters.IsDescending ? query.OrderByDescending(p => p.StockQuantity) : query.OrderBy(p => p.StockQuantity),
            _ => parameters.IsDescending ? query.OrderByDescending(p => p.CreatedAtUtc) : query.OrderByDescending(p => p.CreatedAtUtc)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var pageNumber = parameters.PageNumber < 1 ? 1 : parameters.PageNumber;
        var pageSize = parameters.PageSize < 1 ? 12 : parameters.PageSize;

        var items = await query.Skip((pageNumber - 1) * pageSize)
                               .Take(pageSize)
                               .ToListAsync(cancellationToken);

        return new PagedResult<Product>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<bool> SkuExistsAsync(string sku, Guid? excludeProductId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().Where(p => p.Sku.ToUpper() == sku.ToUpper());
        if (excludeProductId.HasValue)
        {
            query = query.Where(p => p.Id != excludeProductId.Value);
        }
        return await query.AnyAsync(cancellationToken);
    }
}

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Category?> GetWithProductsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(c => c.Products.Where(p => p.IsActive))
                           .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetAllWithProductCountsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(c => c.Products)
                           .AsNoTracking()
                           .OrderBy(c => c.Name)
                           .ToListAsync(cancellationToken);
    }

    public async Task<bool> SlugExistsAsync(string slug, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().Where(c => c.Slug.ToLower() == slug.ToLower());
        if (excludeCategoryId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCategoryId.Value);
        }
        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, Guid? excludeCategoryId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking().Where(c => c.Name.ToLower() == name.ToLower());
        if (excludeCategoryId.HasValue)
        {
            query = query.Where(c => c.Id != excludeCategoryId.Value);
        }
        return await query.AnyAsync(cancellationToken);
    }
}

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Order?> GetWithItemsByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(o => o.User)
                           .Include(o => o.OrderItems)
                           .ThenInclude(oi => oi.Product)
                           .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(o => o.User)
                           .Include(o => o.OrderItems)
                           .ThenInclude(oi => oi.Product)
                           .Where(o => o.UserId == userId)
                           .OrderByDescending(o => o.CreatedAtUtc)
                           .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Order>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.Include(o => o.User)
                           .Include(o => o.OrderItems)
                           .ThenInclude(oi => oi.Product)
                           .OrderByDescending(o => o.CreatedAtUtc)
                           .ToListAsync(cancellationToken);
    }
}

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IUserRepository? _users;
    private IProductRepository? _products;
    private ICategoryRepository? _categories;
    private IOrderRepository? _orders;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IProductRepository Products => _products ??= new ProductRepository(_context);
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);
    public IOrderRepository Orders => _orders ??= new OrderRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
