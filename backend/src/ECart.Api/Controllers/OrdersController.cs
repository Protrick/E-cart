using ECart.Application.DTOs.Orders;
using ECart.Application.Interfaces.Services;
using ECart.Domain.Common;
using ECart.Domain.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECart.Api.Controllers;

[Authorize]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Customer creates a new order
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            throw new UnauthorizedException();
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse.Fail("Validation failed.", errors));
        }

        var order = await _orderService.CreateOrderAsync(userId.Value, dto, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<OrderDto>.Ok(order, "Order placed successfully."));
    }

    /// <summary>
    /// Customer gets their own orders
    /// </summary>
    [HttpGet("my-orders")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrderDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyOrders(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            throw new UnauthorizedException();
        }

        var orders = await _orderService.GetMyOrdersAsync(userId.Value, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<OrderDto>>.Ok(orders, "My orders retrieved successfully."));
    }

    /// <summary>
    /// Admin only: Get all orders across the entire platform
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<OrderDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllOrders(CancellationToken cancellationToken)
    {
        var orders = await _orderService.GetAllOrdersAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<OrderDto>>.Ok(orders, "All orders retrieved successfully."));
    }

    /// <summary>
    /// Get order details by ID (Customer can view their own order, Admin can view any)
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid id, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (!userId.HasValue)
        {
            throw new UnauthorizedException();
        }

        var order = await _orderService.GetOrderByIdAsync(id, userId.Value, IsAdmin(), cancellationToken);
        return Ok(ApiResponse<OrderDto>.Ok(order, "Order retrieved successfully."));
    }

    /// <summary>
    /// Admin only: Update order status (Pending, Processing, Shipped, Delivered, Cancelled)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<OrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusDto dto, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse.Fail("Validation failed.", errors));
        }

        var order = await _orderService.UpdateOrderStatusAsync(id, dto, cancellationToken);
        return Ok(ApiResponse<OrderDto>.Ok(order, "Order status updated successfully."));
    }
}
