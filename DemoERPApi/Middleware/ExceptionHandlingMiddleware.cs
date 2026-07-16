using DemoERPApi.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;


    public ExceptionHandlingMiddleware(
        RequestDelegate next)
    {
        _next = next;
    }



    public async Task Invoke(
        HttpContext context)
    {
        try
        {
            await _next(context);
        }


        catch (NotFoundException ex)
        {
            context.Response.StatusCode =
                StatusCodes.Status404NotFound;


            await context.Response.WriteAsJsonAsync(
                new
                {
                    statusCode = 404,
                    message = ex.Message
                });

            return;
        }


        catch (ValidationException ex)
        {
            context.Response.StatusCode =
                StatusCodes.Status400BadRequest;


            await context.Response.WriteAsJsonAsync(
                new
                {
                    statusCode = 400,
                    message = ex.Message
                });

            return;
        }


        catch (Exception ex)
        {
            context.Response.ContentType =
                "application/problem+json";


            context.Response.StatusCode =
                StatusCodes.Status500InternalServerError;


            var problem =
                new ProblemDetails
                {
                    Status =
                        StatusCodes.Status500InternalServerError,

                    Title =
                        "Internal Server Error",

                    Detail =
                        "An unexpected error occurred.",

                    Instance =
                        context.Request.Path
                };


            problem.Extensions["traceId"] =
                context.TraceIdentifier;


            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
