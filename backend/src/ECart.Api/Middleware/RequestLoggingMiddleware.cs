using System.Diagnostics;

namespace ECart.Api.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var method = request.Method;
        var path = request.Path;
        var query = request.QueryString.ToString();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = context.Response.StatusCode;
            var elapsedMs = stopwatch.ElapsedMilliseconds;

            if (statusCode >= 500)
            {
                _logger.LogError("HTTP {Method} {Path}{Query} responded {StatusCode} in {ElapsedMs} ms [Client: {Ip}]", 
                    method, path, query, statusCode, elapsedMs, ip);
            }
            else if (statusCode >= 400)
            {
                _logger.LogWarning("HTTP {Method} {Path}{Query} responded {StatusCode} in {ElapsedMs} ms [Client: {Ip}]", 
                    method, path, query, statusCode, elapsedMs, ip);
            }
            else
            {
                _logger.LogInformation("HTTP {Method} {Path}{Query} responded {StatusCode} in {ElapsedMs} ms [Client: {Ip}]", 
                    method, path, query, statusCode, elapsedMs, ip);
            }
        }
    }
}
