using System.Net;
using System.Text.Json;
using ECart.Domain.Common;
using ECart.Domain.Exceptions;

namespace ECart.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An unexpected internal server error occurred.";
        List<string>? errors = null;

        switch (exception)
        {
            case AppException appEx:
                statusCode = (HttpStatusCode)appEx.StatusCode;
                message = appEx.Message;
                errors = appEx.Errors;
                _logger.LogWarning(exception, "Handled application exception [{StatusCode}]: {Message}", (int)statusCode, message);
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = "You are not authorized to access this resource.";
                _logger.LogWarning(exception, "Unauthorized access attempt: {Message}", exception.Message);
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = exception.Message;
                _logger.LogWarning(exception, "Resource not found: {Message}", exception.Message);
                break;

            default:
                _logger.LogError(exception, "Unhandled exception occurred: {Message}", exception.Message);
                if (_env.IsDevelopment())
                {
                    message = exception.Message;
                    errors = new List<string> { exception.StackTrace ?? string.Empty };
                }
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse.Fail(message, errors);
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var json = JsonSerializer.Serialize(response, jsonOptions);
        await context.Response.WriteAsync(json);
    }
}
