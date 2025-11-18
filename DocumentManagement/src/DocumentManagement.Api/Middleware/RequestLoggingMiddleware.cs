using System.Diagnostics;

namespace DocumentManagement.Api.Middleware
{
    public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<RequestLoggingMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var requestId = Guid.NewGuid().ToString();

            context.Response.OnStarting(() =>
            {
                context.Response.Headers.Append("X-Request-Id", requestId);
                return Task.CompletedTask;
            });

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();

                var request = context.Request;
                var response = context.Response;

                _logger.LogInformation(
                    "Request {Method} {Path} completed with status {StatusCode} in {ElapsedMilliseconds}ms - RequestId: {RequestId}",
                    request.Method,
                    request.Path,
                    response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    requestId);
            }
        }
    }
}
