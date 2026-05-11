using FluentAssertions;
using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Entities;

namespace MundialitoCorp.UnitTests.Domain.Entities
{
    public class PartidoGolTest
    {
        [Fact]
        public void Constructor_DebeInicializarCorrectamente_CuandoDatosSonValidos()
        {
            // Arrange
            var partidoId = Guid.NewGuid();
            var jugadorId = Guid.NewGuid();

            // Act
            var result = PartidoGol.Create(partidoId, jugadorId);

            // Assert
            Assert.IsType<Result<PartidoGol>>(result);
            result.IsSuccess.Should().BeTrue();
            result.Code.Should().Be(200);
            result.Value!.Id.Should().NotBeEmpty();
            result.Value!.PartidoId.Should().Be(partidoId);
            result.Value!.JugadorId.Should().Be(jugadorId);
        }

        [Fact]
        public void Constructor_RegresaFailure_CuandoPartidoIdEsEmpty()
        {
            // Arrange
            var partidoId = Guid.Empty;
            var jugadorId = Guid.NewGuid();

            // Act
            var result = PartidoGol.Create(partidoId, jugadorId);

            // Assert
            Assert.IsType<Result<PartidoGol>>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(422);
            result.ErrorMessage.Should().Be("El Id del partido es requerido.");
            result.Value.Should().BeNull();
        }

        [Fact]
        public void Constructor_RegresaFailure_CuandoJugadorIdEsEmpty()
        {
            // Arrange
            var partidoId = Guid.NewGuid();
            var jugadorId = Guid.Empty;

            // Act
            var result = PartidoGol.Create(partidoId, jugadorId);

            // Assert
            Assert.IsType<Result<PartidoGol>>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(422);
            result.ErrorMessage.Should().Be("El Id del jugador es requerido.");
            result.Value.Should().BeNull();
        }
    }
}
