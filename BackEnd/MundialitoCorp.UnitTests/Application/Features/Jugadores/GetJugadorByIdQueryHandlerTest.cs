using MundialitoCorp.Application.Features.Jugadores.Queries.GetJugadorById;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Entities;
using FluentAssertions;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Jugadores
{
    public class GetJugadorByIdQueryHandlerTest
    {
        private readonly GetJugadorByIdQueryHandler _handler;
        private readonly Mock<IJugadorQueryService> _jugadorQueryService;

        public GetJugadorByIdQueryHandlerTest()
        {
            _jugadorQueryService = new Mock<IJugadorQueryService>();
            _handler = new GetJugadorByIdQueryHandler(_jugadorQueryService.Object);
        }

        [Fact]
        public async Task Handle_RegresaResultSuccessConJugadorReadModel_CuandoJugadorExiste()
        {
            // Arrange
            Guid jugadorId = Guid.NewGuid();
            var jugador = new JugadorReadModel(jugadorId, "Nombre Jugador", Guid.NewGuid(), "Nombre Equipo", 0);
            var query = new GetJugadorByIdQuery(jugadorId);
            _jugadorQueryService.Setup(s => s.GetByIdAsync(jugadorId)).ReturnsAsync(jugador);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsType<Result<JugadorReadModel>>(result);
            result.IsSuccess.Should().BeTrue();
            result.Code.Should().Be(200);
            result.Value.Should().Be(jugador);
        }

        [Fact]
        public async Task Handle_LlamaCorrectamenteAGetByIdAsync_ConParametrosEnviados()
        {
            // Arrange
            Guid jugadorId = Guid.NewGuid();
            var jugador = new JugadorReadModel(jugadorId, "Nombre Jugador", Guid.NewGuid(), "Nombre Equipo", 0);
            var query = new GetJugadorByIdQuery(jugadorId);
            _jugadorQueryService.Setup(s => s.GetByIdAsync(jugadorId)).ReturnsAsync(jugador);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _jugadorQueryService.Verify(s => s.GetByIdAsync(It.Is<Guid>(x => x == jugadorId)));
        }

        [Fact]
        public async Task Handle_RegresaResultFailure_CuandoJugadorNoExiste()
        {
            // Arrange
            Guid jugadorId = Guid.NewGuid();
            var query = new GetJugadorByIdQuery(jugadorId);
            _jugadorQueryService.Setup(s => s.GetByIdAsync(jugadorId)).ReturnsAsync((JugadorReadModel?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsType<Result<JugadorReadModel>>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(404);
            result.Value.Should().BeNull();
            result.ErrorMessage.Should().Be("El jugador solicitado no existe.");
        }
    }
}
