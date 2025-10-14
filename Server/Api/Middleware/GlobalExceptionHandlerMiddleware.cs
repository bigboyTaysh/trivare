using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Trivare.Application.DTOs.Common;

namespace Trivare.Api.Middleware;

/// <summary>
/// Global exception handling middleware that catches unhandled exceptions
/// and returns consistent error responses following RFC 7807 Problem Details format
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {ExceptionType}", ex.GetType().Name);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var errorResponse = exception switch
        {
            // Database-related exceptions (order matters - most specific first)
            DbUpdateConcurrencyException => new ErrorResponse
            {
                Error = "ConcurrencyError",
                Message = "The record was modified by another user. Please refresh and try again."
            },
            DbUpdateException dbEx => new ErrorResponse
            {
                Error = "DatabaseError",
                Message = "A database error occurred while processing your request.",
                Errors = _environment.IsDevelopment() 
                    ? new Dictionary<string, string[]> { { "Details", new[] { dbEx.Message } } }
                    : null
            },
            
            // Validation exceptions
            ArgumentNullException argEx => new ErrorResponse
            {
                Error = "ValidationError",
                Message = $"Required parameter is missing: {argEx.ParamName}"
            },
            ArgumentException argEx => new ErrorResponse
            {
                Error = "ValidationError",
                Message = argEx.Message
            },
            
            // Operation exceptions
            InvalidOperationException invEx => new ErrorResponse
            {
                Error = "OperationError",
                Message = _environment.IsDevelopment() 
                    ? invEx.Message 
                    : "The operation could not be completed."
            },
            
            // Timeout exceptions
            TimeoutException => new ErrorResponse
            {
                Error = "TimeoutError",
                Message = "The operation timed out. Please try again."
            },
            
            // Generic exception
            _ => new ErrorResponse
            {
                Error = "InternalServerError",
                Message = "An unexpected error occurred. Please try again later.",
                Errors = _environment.IsDevelopment() 
                    ? new Dictionary<string, string[]> 
                    { 
                        { "ExceptionType", new[] { exception.GetType().Name } },
                        { "StackTrace", new[] { exception.StackTrace ?? "No stack trace available" } }
                    }
                    : null
            }
        };

        context.Response.StatusCode = exception switch
        {
            DbUpdateConcurrencyException => (int)HttpStatusCode.Conflict,
            DbUpdateException => (int)HttpStatusCode.BadRequest,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            TimeoutException => (int)HttpStatusCode.RequestTimeout,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var json = JsonSerializer.Serialize(errorResponse, options);
        await context.Response.WriteAsync(json);
    }
}

/// <summary>
/// Extension method for registering the global exception handler middleware
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}
