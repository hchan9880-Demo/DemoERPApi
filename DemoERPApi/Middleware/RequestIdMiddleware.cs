using Microsoft.AspNetCore.Http;

namespace DemoERPApi.Middleware;


/// Middleware that generates or forwards a unique request ID for tracking.

public class RequestIdMiddleware
{
    public const string HeaderName = "X-Request-ID";

    private readonly RequestDelegate _next;

    public RequestIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Reuse incoming request ID or generate new one
        string requestId;
        if (context.Request.Headers.TryGetValue(HeaderName, out var existingId) &&
            !string.IsNullOrWhiteSpace(existingId))
        {
            requestId = existingId.ToString();
        }
        else
        {
            requestId = Guid.NewGuid().ToString();
        }

        // Store for request lifetime
        context.Items["RequestId"] = requestId;

        // Return to caller
        context.Response.Headers[HeaderName] = requestId;

        await _next(context);
    }
}