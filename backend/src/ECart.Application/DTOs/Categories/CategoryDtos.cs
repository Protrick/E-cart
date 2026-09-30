using System.ComponentModel.DataAnnotations;

namespace ECart.Application.DTOs.Categories;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int ProductCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Slug { get; set; }

    public string? ImageUrl { get; set; }
}

public class UpdateCategoryDto
{
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? Slug { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
}
