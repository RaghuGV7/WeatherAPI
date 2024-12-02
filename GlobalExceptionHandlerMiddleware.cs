using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception has occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        object response;

        if (exception is CosmosException cosmosException && cosmosException.StatusCode == HttpStatusCode.NotFound)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            response = new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Resource not found."
            };
            return context.Response.WriteAsJsonAsync(response);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        response = new
        {
            StatusCode = context.Response.StatusCode,
            Message = "Internal Server Error. Please try again later.",
            Detailed = exception.Message // Optionally include detailed error message
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
