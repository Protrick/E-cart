using ECart.Application.DTOs.Categories;
using ECart.Application.Interfaces.Repositories;
using ECart.Application.Interfaces.Services;
using ECart.Domain.Entities;
using ECart.Domain.Exceptions;

namespace ECart.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _unitOfWork.Categories.GetAllWithProductCountsAsync(cancellationToken);
        return categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Slug = c.Slug,
            ImageUrl = c.ImageUrl,
            ProductCount = c.Products?.Count ?? 0,
            CreatedAtUtc = c.CreatedAtUtc
        }).ToList();
    }

    public async Task<CategoryDto> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetWithProductsByIdAsync(id, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException("Category", id);
        }

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            ImageUrl = category.ImageUrl,
            ProductCount = category.Products?.Count ?? 0,
            CreatedAtUtc = category.CreatedAtUtc
        };
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var name = dto.Name.Trim();
        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? GenerateSlug(name)
            : dto.Slug.Trim().ToLowerInvariant();

        if (await _unitOfWork.Categories.NameExistsAsync(name, null, cancellationToken))
        {
            throw new ConflictException($"A category with name '{name}' already exists.");
        }

        if (await _unitOfWork.Categories.SlugExistsAsync(slug, null, cancellationToken))
        {
            throw new ConflictException($"A category with slug '{slug}' already exists.");
        }

        var category = new Category
        {
            Name = name,
            Description = dto.Description.Trim(),
            Slug = slug,
            ImageUrl = dto.ImageUrl?.Trim(),
            IsActive = true
        };

        await _unitOfWork.Categories.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            ImageUrl = category.ImageUrl,
            ProductCount = 0,
            CreatedAtUtc = category.CreatedAtUtc
        };
    }

    public async Task<CategoryDto> UpdateCategoryAsync(Guid id, UpdateCategoryDto dto, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetWithProductsByIdAsync(id, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException("Category", id);
        }

        var name = dto.Name.Trim();
        var slug = string.IsNullOrWhiteSpace(dto.Slug)
            ? GenerateSlug(name)
            : dto.Slug.Trim().ToLowerInvariant();

        if (await _unitOfWork.Categories.NameExistsAsync(name, id, cancellationToken))
        {
            throw new ConflictException($"A category with name '{name}' already exists.");
        }

        if (await _unitOfWork.Categories.SlugExistsAsync(slug, id, cancellationToken))
        {
            throw new ConflictException($"A category with slug '{slug}' already exists.");
        }

        category.Name = name;
        category.Description = dto.Description.Trim();
        category.Slug = slug;
        category.ImageUrl = dto.ImageUrl?.Trim();
        category.IsActive = dto.IsActive;
        category.UpdatedAtUtc = DateTime.UtcNow;

        await _unitOfWork.Categories.UpdateAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            Slug = category.Slug,
            ImageUrl = category.ImageUrl,
            ProductCount = category.Products?.Count ?? 0,
            CreatedAtUtc = category.CreatedAtUtc
        };
    }

    public async Task DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var category = await _unitOfWork.Categories.GetWithProductsByIdAsync(id, cancellationToken);
        if (category == null)
        {
            throw new NotFoundException("Category", id);
        }

        if (category.Products != null && category.Products.Any(p => p.IsActive))
        {
            throw new BadRequestException("Cannot delete category because it contains active products. Please reassign or delete products first.");
        }

        await _unitOfWork.Categories.DeleteAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static string GenerateSlug(string text)
    {
        return text.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("/", "-")
            .Replace("\\", "-");
    }
}
