using ECart.Domain.Common;
using ECart.Domain.Enums;

namespace ECart.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }
    public User? User { get; set; }

    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string ShippingAddress { get; set; } = string.Empty;

    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
