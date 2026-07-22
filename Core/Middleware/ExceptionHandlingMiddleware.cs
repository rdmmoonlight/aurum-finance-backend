using System.Net;
using System.Text.Json;
using Aurum.Api.Core.Exceptions;
using Aurum.Api.Core.Shared;

namespace Aurum.Api.Core.Middleware;

/// <summary>
/// Catches every unhandled exception in the request pipeline and converts it
/// into a flat JSON payload: <c>{ "error": "&lt;message&gt;" }</c>. Every
/// error response in this API uses this same shape — see
/// Core/Shared/ErrorResponse.cs.
/// </summary>
public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
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
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;

        var (statusCode, message) = exception switch
        {
            // Joins every field-level validation message into a single sentence.
            ValidationAppException validationEx => (
                (int)validationEx.StatusCode,
                string.Join("; ", validationEx.Errors.SelectMany(e => e.Value))),

            AppException appEx => (
                (int)appEx.StatusCode,
                appEx.Message),

            _ => (
                (int)HttpStatusCode.InternalServerError,
                _environment.IsDevelopment() ? exception.Message : "An unexpected error occurred.")
        };

        if (statusCode >= 500)
        {
            _logger.LogError(exception, "Unhandled exception for request {TraceId}", traceId);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception for request {TraceId}: {Message}", traceId, exception.Message);
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new ErrorResponse { Error = message }, SerializerOptions));
    }
}
