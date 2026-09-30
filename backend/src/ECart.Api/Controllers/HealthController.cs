using ECart.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace ECart.Api.Controllers;

public class HealthController : BaseApiController
{
    [HttpGet]
    public IActionResult CheckHealth()
    {
        return Ok(ApiResponse<object>.Ok(new
        {
            status = "Healthy",
            serverTimeUtc = DateTime.UtcNow,
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        }, "E-Cart API is healthy and operational."));
    }
}
