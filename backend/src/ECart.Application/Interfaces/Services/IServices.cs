using ECart.Application.DTOs.Auth;
using ECart.Application.DTOs.Categories;
using ECart.Application.DTOs.Orders;
using ECart.Application.DTOs.Products;
using ECart.Domain.Common;
using ECart.Domain.Entities;

namespace ECart.Application.Interfaces.Services;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, out DateTime expiresAt);
}

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
}

public interface IProductService
{
    Task<PagedResult<ProductDto>> GetProductsAsync(ProductQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<ProductDto> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default);
    Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default);
    Task DeleteProductAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface ICategoryService
{
    Task<IReadOnlyList<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<CategoryDto> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default);
    Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);
}

public interface IOrderService
{
    Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderDto dto, CancellationToken cancellationToken = default);
    Task<OrderDto> GetOrderByIdAsync(Guid orderId, Guid requestingUserId, bool isAdmin, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetMyOrdersAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default);
    Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto dto, CancellationToken cancellationToken = default);
}
