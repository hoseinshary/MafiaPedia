using System.Net;
using System.Text.Json;
using MafiaPedia.Api.Common.Exceptions;

namespace MafiaPedia.Api.Middleware
{
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
                var (statusCode, message) = ex switch
                {
                    NotFoundAppException => (HttpStatusCode.NotFound, ex.Message),
                    ConflictAppException => (HttpStatusCode.Conflict, ex.Message),
                    ForbiddenAppException => (HttpStatusCode.Forbidden, ex.Message),
                    ValidationAppException => (HttpStatusCode.BadRequest, ex.Message),
                    _ => (HttpStatusCode.InternalServerError, "خطای داخلی سرور رخ داده است.")
                };

                if (statusCode == HttpStatusCode.InternalServerError)
                {
                    _logger.LogError(ex,
                        "Unhandled exception on {Method} {Path} | QueryString: {QueryString} | TraceId: {TraceId}",
                        context.Request.Method,
                        context.Request.Path,
                        context.Request.QueryString,
                        context.TraceIdentifier);
                }
                else
                {
                    _logger.LogWarning(
                        "Handled exception {ExceptionType} on {Method} {Path} | TraceId: {TraceId} | Message: {Message}",
                        ex.GetType().Name,
                        context.Request.Method,
                        context.Request.Path,
                        context.TraceIdentifier,
                        ex.Message);
                }

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)statusCode;

                var payload = JsonSerializer.Serialize(new
                {
                    message,
                    traceId = context.TraceIdentifier
                });

                await context.Response.WriteAsync(payload);
            }
        }
    }
}
