using DocumentManagement.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace DocumentManagement.Api.Middleware
{
    public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "An unhandled exception occurred");

            var (statusCode, message) = exception switch
            {
                DocumentNotFoundException => (HttpStatusCode.NotFound, exception.Message),
                Domain.Exceptions.UnauthorizedAccessException => (HttpStatusCode.Forbidden, exception.Message),
                InvalidFileException => (HttpStatusCode.BadRequest, exception.Message),
                FileSizeExceededException => (HttpStatusCode.BadRequest, exception.Message),
                ValidationException => (HttpStatusCode.BadRequest, exception.Message),
                _ => (HttpStatusCode.InternalServerError, "An internal server error occurred.")
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                error = message,
                statusCode = (int)statusCode,
                timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
