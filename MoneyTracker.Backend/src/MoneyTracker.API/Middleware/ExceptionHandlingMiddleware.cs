using System.Net;
using System.Text.Json;
using MoneyTracker.Application.Common.Exceptions;

namespace MoneyTracker.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = context.Response;

        object errorResponse;

        switch (exception)
        {
            case ValidationException ex:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse = new
                {
                    message = "Validation failed",
                    statusCode = response.StatusCode,
                    errors = ex.Errors
                };
                break;
                
            case NotFoundException ex:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse = new
                {
                    message = ex.Message,
                    statusCode = response.StatusCode
                };
                break;
                
            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse = new
                {
                    message = "An error occurred while processing your request.",
                    statusCode = response.StatusCode
                };
                break;
        }

        var jsonResponse = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(jsonResponse);
    }
}