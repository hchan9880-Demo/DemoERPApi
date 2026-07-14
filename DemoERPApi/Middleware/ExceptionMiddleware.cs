using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace DemoERPApi.Middleware;


public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;


    public ExceptionMiddleware(
        RequestDelegate next,
        ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }



    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Unhandled exception occurred");


            context.Response.Clear();


            context.Response.StatusCode =
                StatusCodes.Status500InternalServerError;


            context.Response.ContentType =
                "application/problem+json";



            var problem =
                new ProblemDetails
                {
                    Status = 500,

                    Title =
                        "Internal Server Error",

                    Detail =
                        "An unexpected error occurred.",

                    Instance =
                        context.Request.Path
                };


            await context.Response.WriteAsync(
                JsonSerializer.Serialize(problem));
        }
    }
}