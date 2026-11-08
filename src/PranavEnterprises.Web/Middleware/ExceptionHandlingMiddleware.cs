using System.Net;
using System.Text.Json;

namespace PranavEnterprises.Web.Middleware;

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
            _logger.LogError(ex, "Unhandled exception occurred while processing request: {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var isApi = context.Request.Path.StartsWithSegments("/api");

        if (isApi)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                isSuccess = false,
                message = "An internal server error occurred while processing your request.",
                details = exception.Message,
                statusCode = context.Response.StatusCode,
                traceId = context.TraceIdentifier,
                timestamp = DateTime.UtcNow
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        // For MVC web pages, re-throw or redirect to error page
        context.Response.Redirect($"/Home/Error?traceId={context.TraceIdentifier}");
        return Task.CompletedTask;
    }
}
