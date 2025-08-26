// MoneyTracker.API/Middleware/ExceptionHandlingMiddleware.cs
using System.Net;
using System.Text.Json;
using MoneyTracker.Application.Common.Exceptions;
using MoneyTracker.API.Models;

namespace MoneyTracker.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred. Path: {Path}, Method: {Method}",
                context.Request.Path, context.Request.Method);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            ValidationException validationEx => ApiResponse<object>.ErrorResult(
                "Validation fehlgeschlagen", // German error message
                validationEx.Errors.Select(e => new ValidationError
                {
                    PropertyName = e.Key,
                    //ErrorMessage = e.Value,
                    AttemptedValue = e.Value
                }).ToList(),
                (int)HttpStatusCode.BadRequest),

            NotFoundException notFoundEx => ApiResponse<object>.ErrorResult(
                notFoundEx.Message,
                null,
                (int)HttpStatusCode.NotFound),

            UnauthorizedAccessException => ApiResponse<object>.ErrorResult(
                "Nicht autorisierter Zugriff", // German error message
                null,
                (int)HttpStatusCode.Unauthorized),

            ArgumentException argEx => ApiResponse<object>.ErrorResult(
                $"Ungültiges Argument: {argEx.Message}", // German error message
                null,
                (int)HttpStatusCode.BadRequest),

            InvalidOperationException invalidEx => ApiResponse<object>.ErrorResult(
                $"Ungültige Operation: {invalidEx.Message}", // German error message
                null,
                (int)HttpStatusCode.BadRequest),

            _ => ApiResponse<object>.ErrorResult(
                "Ein Fehler ist bei der Verarbeitung Ihrer Anfrage aufgetreten", // German error message
                null,
                (int)HttpStatusCode.InternalServerError)
        };

        context.Response.StatusCode = response.StatusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        var jsonResponse = JsonSerializer.Serialize(response, options);
        await context.Response.WriteAsync(jsonResponse);
    }
}