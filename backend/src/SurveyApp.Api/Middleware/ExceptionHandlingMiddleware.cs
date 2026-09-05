using System.Net;
using System.Text.Json;

namespace SurveyApp.Api.Middleware;

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
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            KeyNotFoundException => HttpStatusCode.NotFound,
            InvalidOperationException => HttpStatusCode.Conflict,
            ArgumentException => HttpStatusCode.BadRequest,
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            _ => HttpStatusCode.InternalServerError
        };

        var message = statusCode == HttpStatusCode.InternalServerError
            ? "Sunucu tarafında beklenmeyen bir hata oluştu."
            : exception.Message;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new { message };
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}