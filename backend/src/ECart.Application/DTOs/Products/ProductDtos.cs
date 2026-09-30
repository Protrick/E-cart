using System.ComponentModel.DataAnnotations;
using ECart.Application.DTOs.Categories;

namespace ECart.Application.DTOs.Products;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public class CreateProductDto
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product description is required")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "SKU is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "SKU must be between 3 and 50 characters")]
    public string Sku { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 1000000.00, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, 1000000, ErrorMessage = "Stock quantity cannot be negative")]
    public int StockQuantity { get; set; }

    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Category ID is required")]
    public Guid CategoryId { get; set; }
}

public class UpdateProductDto
{
    [Required(ErrorMessage = "Product name is required")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product description is required")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "SKU is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "SKU must be between 3 and 50 characters")]
    public string Sku { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required")]
    [Range(0.01, 1000000.00, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required(ErrorMessage = "Stock quantity is required")]
    [Range(0, 1000000, ErrorMessage = "Stock quantity cannot be negative")]
    public int StockQuantity { get; set; }

    public string? ImageUrl { get; set; }

    [Required(ErrorMessage = "Category ID is required")]
    public Guid CategoryId { get; set; }

    public bool IsActive { get; set; } = true;
}

public class ProductQueryParameters
{
    public string? Search { get; set; }
    public Guid? CategoryId { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStockOnly { get; set; }
    public string? SortBy { get; set; } // "name", "price", "createdAt", "stock"
    public bool IsDescending { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}
