using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Entities;
using FluentAssertions;

namespace MundialitoCorp.UnitTests.Domain.Common
{
    public class ResultTests
    {
        [Fact]
        public void Success_DebeRetornarEstadoExitoso_SinErrores()
        {
            // Act
            var result = Result.Success(200);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.ErrorMessage.Should().BeNull();
        }

        [Fact]
        public void Failure_DebeRetornarEstadoFallido_ConMensajeYCodigo()
        {
            // Arrange
            var mensaje = "Error de prueba";
            var codigo = 400;

            // Act
            var result = Result.Failure(mensaje, codigo);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be(mensaje);
            result.Code.Should().Be(codigo);
        }

        [Fact]
        public void Success_Generico_DebeContenerElValorAsignado()
        {
            // Arrange
            var valor = 100;

            // Act
            var result = Result<int>.Success(valor, 200);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IsFailure.Should().BeFalse();
            result.Value.Should().Be(valor);
        }

        [Fact]
        public void Failure_ConTipoValor_DebeRetornarCero()
        {
            var result = Result<int>.Failure("Error", 422);
            result.Value.Should().Be(0);
        }

        [Fact]
        public void Failure_ConClase_DebeRetornarNull()
        {
            var result = Result<Equipo>.Failure("Error", 422);
            result.Value.Should().BeNull();
        }
    }
}