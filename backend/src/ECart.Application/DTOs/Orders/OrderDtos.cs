using System.ComponentModel.DataAnnotations;
using ECart.Domain.Enums;

namespace ECart.Application.DTOs.Orders;

public class OrderDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusText => Status.ToString();
    public string ShippingAddress { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ProductImageUrl { get; set; }
    public string Sku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreateOrderItemDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(1, 1000, ErrorMessage = "Quantity must be at least 1")]
    public int Quantity { get; set; }
}

public class CreateOrderDto
{
    [Required(ErrorMessage = "Shipping address is required")]
    [StringLength(500, MinimumLength = 5, ErrorMessage = "Shipping address must be between 5 and 500 characters")]
    public string ShippingAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "At least one item is required in the order")]
    [MinLength(1, ErrorMessage = "Order must contain at least one item")]
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class UpdateOrderStatusDto
{
    [Required(ErrorMessage = "Order status is required")]
    public OrderStatus Status { get; set; }
}
