using System.Text.Json;
using MundialitoCorp.Api.Filters;
using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Domain.Common;
using MundialitoCorp.Infrastructure.Persistence;
using MundialitoCorp.Infrastructure.Persistence.Idempotency;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace MundialitoCorp.UnitTests.Api.Filters
{
    public class IdempotencyFilterTests
    {
        private readonly Mock<IIdempotencyService> _idempotencyServiceMock;
        private readonly IdempotencyFilterAttribute _filter;

        public IdempotencyFilterTests()
        {
            _idempotencyServiceMock = new Mock<IIdempotencyService>();
            _filter = new IdempotencyFilterAttribute();
        }

        [Fact]
        public async Task OnActionExecutionAsync_SiNoEnviaHeader_RetornaBadRequest()
        {
            // Arrange
            var context = CreateContext(keyHeaderValue: null);
            ActionExecutionDelegate next = () => Task.FromResult<ActionExecutedContext>(null!);

            // Act
            await _filter.OnActionExecutionAsync(context, next);

            // Assert
            var result = Assert.IsType<BadRequestObjectResult>(context.Result);
            var resultValue = Assert.IsType<Result>(result.Value);
            Assert.Contains("obligatorio", resultValue.ErrorMessage);
        }

        [Fact]
        public async Task OnActionExecutionAsync_SiLlaveEsInvalida_RetornaBadRequest()
        {
            // Arrange
            var context = CreateContext(keyHeaderValue: "esto-no-es-un-guid");
            ActionExecutionDelegate next = () => Task.FromResult<ActionExecutedContext>(null!);

            // Act
            await _filter.OnActionExecutionAsync(context, next);

            // Assert
            Assert.IsType<BadRequestObjectResult>(context.Result);
        }

        [Fact]
        public async Task OnActionExecutionAsync_SiLlaveYaExiste_RetornaResultadoGuardadoYNoEjecutaElControlador()
        {
            // Arrange
            var key = Guid.NewGuid();
            var context = CreateContext(key.ToString());
            var storedResponse = JsonSerializer.Serialize(Result.Success(200));

            _idempotencyServiceMock.Setup(s => s.GetRequestAsync(key))
                .ReturnsAsync(new IdempotencyResponse(key, storedResponse, 200));

            bool nextCalled = false;
            ActionExecutionDelegate next = () => {
                nextCalled = true;
                return Task.FromResult<ActionExecutedContext>(null!);
            };

            // Act
            await _filter.OnActionExecutionAsync(context, next);

            // Assert
            var result = Assert.IsType<ContentResult>(context.Result);
            Assert.Equal(storedResponse, result.Content);
            Assert.Equal(200, result.StatusCode);
            Assert.False(nextCalled);
        }

        [Fact]
        public async Task OnActionExecutionAsync_SiEsNuevaPeticion_EjecutaControladorYGuardaResultado()
        {
            // Arrange
            var key = Guid.NewGuid();
            var context = CreateContext(key.ToString());
            _idempotencyServiceMock.Setup(s => s.GetRequestAsync(key)).ReturnsAsync((IdempotencyResponse?)null);

            var apiResult = Result.Success(200);
            var objectResult = new ObjectResult(apiResult) { StatusCode = 200 };

            ActionExecutionDelegate next = () => {
                var executedContext = new ActionExecutedContext(context, new List<IFilterMetadata>(), new Mock<Controller>().Object)
                {
                    Result = objectResult
                };
                return Task.FromResult(executedContext);
            };

            // Act
            await _filter.OnActionExecutionAsync(context, next);

            // Assert
            _idempotencyServiceMock.Verify(s => s.CreateRequestAsync(
                key,
                It.IsAny<string>(),
                It.IsAny<string>(),
                200), Times.Once);
        }

        [Fact]
        public async Task CreateRequestAsync_DebePersistirKey_Y_GetRequestDebeRetornarla()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var key = Guid.NewGuid();
            var path = "/api/partidos/registrar-resultado";
            var responseBody = "{\"isSuccess\":true}";
            var statusCode = 200;

            // Act
            using (var contextWrite = new ApplicationDbContext(options))
            {
                var service = new IdempotencyService(contextWrite);
                await service.CreateRequestAsync(key, path, responseBody, statusCode);
            }

            // Assert
            using (var contextRead = new ApplicationDbContext(options))
            {
                var serviceRead = new IdempotencyService(contextRead);

                var result = await serviceRead.GetRequestAsync(key);

                result.Should().NotBeNull();
                result!.Key.Should().Be(key);
                result.ResponseBody.Should().Be(responseBody);
                result.StatusCode.Should().Be(statusCode);
            }
        }


        private ActionExecutingContext CreateContext(string? keyHeaderValue)
        {
            var httpContext = new DefaultHttpContext();
            if (keyHeaderValue != null)
                httpContext.Request.Headers["Idempotency-Key"] = keyHeaderValue;

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(x => x.GetService(typeof(IIdempotencyService)))
                           .Returns(_idempotencyServiceMock.Object);
            httpContext.RequestServices = serviceProvider.Object;

            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            return new ActionExecutingContext(
                actionContext,
                new List<IFilterMetadata>(),
                new Dictionary<string, object?>(),
                new Mock<Controller>().Object);
        }
    }
}
