using ECart.Application.DTOs.Orders;
using ECart.Application.Interfaces.Repositories;
using ECart.Application.Interfaces.Services;
using ECart.Domain.Entities;
using ECart.Domain.Enums;
using ECart.Domain.Exceptions;

namespace ECart.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OrderDto> CreateOrderAsync(Guid userId, CreateOrderDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null || !user.IsActive)
        {
            throw new NotFoundException("User not found or account is inactive.");
        }

        if (dto.Items == null || dto.Items.Count == 0)
        {
            throw new BadRequestException("Order must contain at least one item.");
        }

        var order = new Order
        {
            UserId = userId,
            ShippingAddress = dto.ShippingAddress.Trim(),
            Status = OrderStatus.Pending,
            TotalAmount = 0
        };

        decimal totalAmount = 0;

        foreach (var itemDto in dto.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId, cancellationToken);
            if (product == null || !product.IsActive)
            {
                throw new BadRequestException($"Product with ID '{itemDto.ProductId}' does not exist or is inactive.");
            }

            if (product.StockQuantity < itemDto.Quantity)
            {
                throw new BadRequestException($"Not enough stock for product '{product.Name}'. Available: {product.StockQuantity}, Requested: {itemDto.Quantity}.");
            }

            // Deduct stock
            product.StockQuantity -= itemDto.Quantity;
            await _unitOfWork.Products.UpdateAsync(product, cancellationToken);

            var itemTotal = product.Price * itemDto.Quantity;
            totalAmount += itemTotal;

            order.OrderItems.Add(new OrderItem
            {
                ProductId = product.Id,
                Product = product,
                Quantity = itemDto.Quantity,
                UnitPrice = product.Price,
                TotalPrice = itemTotal
            });
        }

        order.TotalAmount = totalAmount;

        await _unitOfWork.Orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        order.User = user;
        return MapToOrderDto(order);
    }

    public async Task<OrderDto> GetOrderByIdAsync(Guid orderId, Guid requestingUserId, bool isAdmin, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetWithItemsByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new NotFoundException("Order", orderId);
        }

        if (!isAdmin && order.UserId != requestingUserId)
        {
            throw new ForbiddenException("You are not authorized to view this order.");
        }

        return MapToOrderDto(order);
    }

    public async Task<IReadOnlyList<OrderDto>> GetMyOrdersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetByUserIdAsync(userId, cancellationToken);
        return orders.Select(MapToOrderDto).ToList();
    }

    public async Task<IReadOnlyList<OrderDto>> GetAllOrdersAsync(CancellationToken cancellationToken = default)
    {
        var orders = await _unitOfWork.Orders.GetAllWithDetailsAsync(cancellationToken);
        return orders.Select(MapToOrderDto).ToList();
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto dto, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetWithItemsByIdAsync(orderId, cancellationToken);
        if (order == null)
        {
            throw new NotFoundException("Order", orderId);
        }

        // If order was cancelled now, restock items
        if (dto.Status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
        {
            foreach (var item in order.OrderItems)
            {
                if (item.Product != null)
                {
                    item.Product.StockQuantity += item.Quantity;
                    await _unitOfWork.Products.UpdateAsync(item.Product, cancellationToken);
                }
            }
        }

        order.Status = dto.Status;
        order.UpdatedAtUtc = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToOrderDto(order);
    }

    private static OrderDto MapToOrderDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            UserEmail = order.User?.Email ?? string.Empty,
            UserName = order.User != null ? $"{order.User.FirstName} {order.User.LastName}".Trim() : string.Empty,
            TotalAmount = order.TotalAmount,
            Status = order.Status,
            ShippingAddress = order.ShippingAddress,
            CreatedAtUtc = order.CreatedAtUtc,
            Items = order.OrderItems.Select(oi => new OrderItemDto
            {
                Id = oi.Id,
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? string.Empty,
                ProductImageUrl = oi.Product?.ImageUrl,
                Sku = oi.Product?.Sku ?? string.Empty,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice
            }).ToList()
        };
    }
}
