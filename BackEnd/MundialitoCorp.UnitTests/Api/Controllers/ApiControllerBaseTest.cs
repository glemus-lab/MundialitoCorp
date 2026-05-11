using MediatR;
using Microsoft.AspNetCore.Mvc;
using MundialitoCorp.Api.Controllers;
using MundialitoCorp.Domain.Common;
using Moq;
using FluentAssertions;

namespace MundialitoCorp.UnitTests.Api.Controllers
{
    public class ApiControllerBaseTests
    {
        private class TestController : ApiControllerBase
        {
            public TestController(ISender mediator) : base(mediator) { }
            public ActionResult ExecuteHandle(Result result) => HandleResult(result);
            public ActionResult ExecuteHandleGeneric<T>(Result<T> result) => HandleResult(result);
        }

        private readonly TestController _controller;

        public ApiControllerBaseTests()
        {
            _controller = new TestController(new Mock<ISender>().Object);
        }

        [Theory]
        [InlineData(200, 200)]
        [InlineData(201, 201)]
        [InlineData(204, 200)]
        [InlineData(400, 400)]
        [InlineData(401, 401)]
        [InlineData(403, 403)]
        [InlineData(404, 404)]
        [InlineData(409, 409)]
        [InlineData(500, 500)]
        public void HandleResult_DebeRetornarStatusCodeCorrecto_SegunMatrizDeEntrada(int codeIn, int expectedCode)
        {
            // Arrange
            var result = Result.Failure("Test", codeIn);

            // Act
            var actionResult = _controller.ExecuteHandle(result) as ObjectResult;

            // Assert
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(expectedCode);
        }

        [Fact]
        public void HandleResult_Success_DebeRetornarOkConResultEnCuerpo()
        {
            // Arrange
            var result = Result.Success(200);

            // Act
            var actionResult = _controller.ExecuteHandle(result) as ObjectResult;

            // Assert
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(200);
            actionResult.Value.Should().BeAssignableTo<Result>();
            var valor = (Result)actionResult.Value!;
            valor.IsSuccess.Should().BeTrue();
            valor.Code.Should().Be(200);
        }

        [Fact]
        public void HandleResultGeneric_Success_DebeRetornarOkConDataYResultEnCuerpo()
        {
            // Arrange
            var data = new { Nombre = "Equipo Test" };
            var result = Result<object>.Success(data, 200);

            // Act
            var actionResult = _controller.ExecuteHandleGeneric(result) as ObjectResult;

            // Assert
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(200);
            actionResult.Value.Should().BeAssignableTo<Result<object>>();
            var valor = actionResult.Value as Result<object>;
            valor!.Value.Should().BeEquivalentTo(data);
            valor.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public void HandleResult_Failure_DebeRetornarStatusCodeErrorYMensajeEnCuerpo()
        {
            // Arrange
            var mensaje = "Error de validación";
            var result = Result.Failure(mensaje, 400);

            // Act
            var actionResult = _controller.ExecuteHandle(result) as ObjectResult;

            // Assert
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(400);
            var valor = actionResult.Value as Result;
            valor!.IsSuccess.Should().BeFalse();
            valor.ErrorMessage.Should().Be(mensaje);
        }

        [Fact]
        public void HandleResultGeneric_Failure_DebeRetornarCuerpoConValorNuloYEstadoFalso()
        {
            // Arrange
            var result = Result<string>.Failure("No encontrado", 404);

            // Act
            var actionResult = _controller.ExecuteHandleGeneric(result) as ObjectResult;

            // Assert
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(404);
            var valor = actionResult.Value as Result<string>;
            valor!.IsSuccess.Should().BeFalse();
            valor.Value.Should().BeNull();
            valor.ErrorMessage.Should().Be("No encontrado");
        }

        [Fact]
        public void HandleResult_NoContent_DebeTransformar204En200SegunLogicaInterna()
        {
            // Arrange
            var result = Result.Success(204);

            // Act
            var actionResult = _controller.ExecuteHandle(result) as ObjectResult;

            // Assert
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(200);
        }

        [Fact]
        public void HandleResult_ServerError_DebeRetornar500YMantenerObjetoResult()
        {
            // Arrange
            var result = Result.Failure("Error interno", 500);

            // Act
            var actionResult = _controller.ExecuteHandle(result) as ObjectResult;

            // Assert
            actionResult.Should().NotBeNull();
            actionResult!.StatusCode.Should().Be(500);
            var valor = actionResult.Value as Result;
            valor!.IsSuccess.Should().BeFalse();
        }
    }
}