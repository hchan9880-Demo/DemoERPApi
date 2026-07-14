using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace DemoERPApi.Middleware
{
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
            string requestId;

            // Reuse an incoming Request ID if one was supplied
            if (context.Request.Headers.TryGetValue(HeaderName, out var existingId)
                && !string.IsNullOrWhiteSpace(existingId))
            {
                requestId = existingId.ToString();
            }
            else
            {
                requestId = Guid.NewGuid().ToString();
            }

            // Store for the lifetime of the request
            context.Items["RequestId"] = requestId;

            // Return it to the caller
            context.Response.Headers[HeaderName] = requestId;

            await _next(context);
        }
    }
}