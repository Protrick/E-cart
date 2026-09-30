using ECart.Application.DTOs.Products;
using ECart.Application.Interfaces.Repositories;
using ECart.Application.Interfaces.Services;
using ECart.Domain.Common;
using ECart.Domain.Entities;
using ECart.Domain.Exceptions;

namespace ECart.Application.Services;

public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<ProductDto>> GetProductsAsync(ProductQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var pagedEntities = await _unitOfWork.Products.GetPagedProductsAsync(parameters, cancellationToken);
        var dtos = pagedEntities.Items.Select(MapToProductDto).ToList();

        return new PagedResult<ProductDto>(dtos, pagedEntities.TotalCount, pagedEntities.PageNumber, pagedEntities.PageSize);
    }

    public async Task<ProductDto> GetProductByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetWithCategoryByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException("Product", id);
        }

        return MapToProductDto(product);
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductDto dto, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException("Category", dto.CategoryId);
        }

        var normalizedSku = dto.Sku.Trim().ToUpperInvariant();
        var skuExists = await _unitOfWork.Products.SkuExistsAsync(normalizedSku, null, cancellationToken);
        if (skuExists)
        {
            throw new ConflictException($"A product with SKU '{dto.Sku}' already exists.");
        }

        var product = new Product
        {
            Name = dto.Name.Trim(),
            Description = dto.Description.Trim(),
            Sku = normalizedSku,
            Price = dto.Price,
            StockQuantity = dto.StockQuantity,
            ImageUrl = dto.ImageUrl?.Trim(),
            CategoryId = dto.CategoryId,
            IsActive = true
        };

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        product.Category = category;
        return MapToProductDto(product);
    }

    public async Task<ProductDto> UpdateProductAsync(Guid id, UpdateProductDto dto, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetWithCategoryByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException("Product", id);
        }

        var category = await _unitOfWork.Categories.GetByIdAsync(dto.CategoryId, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException("Category", dto.CategoryId);
        }

        var normalizedSku = dto.Sku.Trim().ToUpperInvariant();
        var skuExists = await _unitOfWork.Products.SkuExistsAsync(normalizedSku, id, cancellationToken);
        if (skuExists)
        {
            throw new ConflictException($"A product with SKU '{dto.Sku}' already exists.");
        }

        product.Name = dto.Name.Trim();
        product.Description = dto.Description.Trim();
        product.Sku = normalizedSku;
        product.Price = dto.Price;
        product.StockQuantity = dto.StockQuantity;
        product.ImageUrl = dto.ImageUrl?.Trim();
        product.CategoryId = dto.CategoryId;
        product.IsActive = dto.IsActive;
        product.UpdatedAtUtc = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        product.Category = category;
        return MapToProductDto(product);
    }

    public async Task DeleteProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            throw new NotFoundException("Product", id);
        }

        await _unitOfWork.Products.DeleteAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ProductDto MapToProductDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Sku = product.Sku,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            ImageUrl = product.ImageUrl,
            IsActive = product.IsActive,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty,
            CreatedAtUtc = product.CreatedAtUtc,
            UpdatedAtUtc = product.UpdatedAtUtc
        };
    }
}
