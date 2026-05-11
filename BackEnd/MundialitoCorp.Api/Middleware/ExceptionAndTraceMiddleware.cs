using System.Diagnostics;
using System.Net;
using System.Text.Json;
using MundialitoCorp.Domain.Common;

namespace MundialitoCorp.Api.Middleware
{
    public class ExceptionAndTraceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionAndTraceMiddleware> _logger;

        public ExceptionAndTraceMiddleware(RequestDelegate next, ILogger<ExceptionAndTraceMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var timer = Stopwatch.StartNew();
            var correlationId = context.Request.Headers["X-Correlation-Id"].ToString();

            if (string.IsNullOrEmpty(correlationId))
                correlationId = Guid.NewGuid().ToString();

            using (_logger.BeginScope("TraceId:{TraceId}", correlationId))
            {
                context.Items["CorrelationId"] = correlationId;
                context.Response.Headers.Append("X-Correlation-Id", correlationId);

                try
                {
                    _logger.LogInformation("Iniciando Request {Method} {Path}", context.Request.Method, context.Request.Path);
                    await _next(context);
                    timer.Stop();
                    _logger.LogInformation("Request {Method} {Path} completado en {Duration}ms",
                        context.Request.Method, context.Request.Path, timer.ElapsedMilliseconds);
                }
                catch (Exception ex)
                {
                    timer.Stop();
                    _logger.LogError(ex, "Error no controlado durante la petición {Method} {Path}", context.Request.Method, context.Request.Path);
                    await HandleExceptionAsync(context, ex);
                }
            }
        }

        public Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            _logger.LogError("Ocurrio un error interno. Exception: {Exception}", exception.Message);
            var response = Result.Failure("Ha ocurrido un error interno en el servidor.", context.Response.StatusCode);
            return context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
        }
    }
}
