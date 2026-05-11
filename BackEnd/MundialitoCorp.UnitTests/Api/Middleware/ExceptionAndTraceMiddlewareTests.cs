using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using MundialitoCorp.Api.Middleware;
using MundialitoCorp.Domain.Common;

namespace MundialitoCorp.UnitTests.Api.Middleware
{
    public class ExceptionAndTraceMiddlewareTests
    {
        private readonly Mock<ILogger<ExceptionAndTraceMiddleware>> _loggerMock;
        private readonly DefaultHttpContext _context;

        public ExceptionAndTraceMiddlewareTests()
        {
            _loggerMock = new Mock<ILogger<ExceptionAndTraceMiddleware>>();
            _context = new DefaultHttpContext();

            _context.Response.Body = new MemoryStream();
        }

        [Fact]
        public async Task InvokeAsync_DebeAsignarCorrelationIdExistente()
        {
            // Arrange
            var existingCorrelationId = Guid.NewGuid().ToString();
            _context.Request.Headers["X-Correlation-Id"] = existingCorrelationId;
            RequestDelegate next = (innerContext) => Task.CompletedTask;
            var middleware = new ExceptionAndTraceMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            _context.Response.Headers["X-Correlation-Id"].ToString().Should().Be(existingCorrelationId);
            _context.Items["CorrelationId"].Should().Be(existingCorrelationId);
        }

        [Fact]
        public async Task InvokeAsync_DebeGenerarNuevoCorrelationId_SiNoVieneEnHeader()
        {
            // Arrange
            RequestDelegate next = (innerContext) => Task.CompletedTask;
            var middleware = new ExceptionAndTraceMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            _context.Response.Headers["X-Correlation-Id"].ToString().Should().NotBeEmpty();
            _context.Items["CorrelationId"]!.ToString().Should().NotBeEmpty();
        }

        [Fact]
        public async Task InvokeAsync_DebeCapturarExcepcionYRetornarInternalServerError()
        {
            var exceptionMessage = "Error de prueba";
            RequestDelegate next = (innerContext) => throw new Exception(exceptionMessage);
            var middleware = new ExceptionAndTraceMiddleware(next, _loggerMock.Object);

            await middleware.InvokeAsync(_context);

            _context.Response.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            _context.Response.ContentType.Should().Be("application/json");

            _context.Response.Body.Seek(0, SeekOrigin.Begin);
            using var reader = new StreamReader(_context.Response.Body);
            var body = await reader.ReadToEndAsync();

            var definition = new { isSuccess = false, errorMessage = "", code = 0 };
            var result = JsonSerializer.Deserialize(body, definition.GetType(), new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            result.Should().NotBeNull();

            var isSuccess = (bool)result!.GetType().GetProperty("isSuccess")!.GetValue(result)!;
            var message = (string)result!.GetType().GetProperty("errorMessage")!.GetValue(result)!;
            var code = (int)result!.GetType().GetProperty("code")!.GetValue(result)!;

            isSuccess.Should().BeFalse();
            message.Should().Be("Ha ocurrido un error interno en el servidor.");
            code.Should().Be(500);
        }

        [Fact]
        public async Task InvokeAsync_DebeLoguearInicioYFinDePeticion()
        {
            // Arrange
            RequestDelegate next = (innerContext) => Task.CompletedTask;
            var middleware = new ExceptionAndTraceMiddleware(next, _loggerMock.Object);

            // Act
            await middleware.InvokeAsync(_context);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Iniciando Request")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("completado en")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}