using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace ECart.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    protected Guid? GetCurrentUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? User.FindFirst("sub")?.Value;

        if (Guid.TryParse(idClaim, out var userId))
        {
            return userId;
        }

        return null;
    }

    protected string? GetCurrentUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)?.Value 
            ?? User.FindFirst("role")?.Value;
    }

    protected bool IsAdmin()
    {
        return string.Equals(GetCurrentUserRole(), "Admin", StringComparison.OrdinalIgnoreCase);
    }
}
