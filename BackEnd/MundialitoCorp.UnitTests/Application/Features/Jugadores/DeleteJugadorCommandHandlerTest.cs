using MundialitoCorp.Application.Features.Jugadores.Commands.DeleteJugador;
using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Jugadores
{
    public class DeleteJugadorCommandHandlerTest
    {
        private readonly DeleteJugadorCommandHandler _handler;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IJugadorRepository> _jugadorRepository;
        private readonly Mock<ILogger<DeleteJugadorCommandHandler>> _logger;

        public DeleteJugadorCommandHandlerTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _jugadorRepository = new Mock<IJugadorRepository>();
            _logger = new Mock<ILogger<DeleteJugadorCommandHandler>>();
            _handler = new DeleteJugadorCommandHandler(_unitOfWork.Object, _jugadorRepository.Object, _logger.Object);
        }

        [Fact]
        public async Task Handle_RegresaResultSuccess_CuandoEliminaCorrectamenteUnJugador()
        {
            // Arrange
            var jugadorId = Guid.NewGuid();
            var command = new DeleteJugadorCommand(jugadorId);
            var jugador = Jugador.Create("Nombre Jugador", Guid.NewGuid()).Value!;
            _jugadorRepository.Setup(s => s.GetByIdAsync(jugadorId)).ReturnsAsync(jugador);
            _jugadorRepository.Setup(s => s.DeleteAsync(jugadorId));
            _unitOfWork.Setup(s => s.SaveChangesAsync());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsSuccess.Should().BeTrue();
            result.Code.Should().Be(204);
        }

        [Fact]
        public async Task Handle_RegresaResultFailure_CuandoJugadorNoExiste()
        {
            // Arrange
            var jugadorId = Guid.NewGuid();
            var command = new DeleteJugadorCommand(jugadorId);
            var jugador = Jugador.Create("Nombre Jugador", Guid.NewGuid()).Value!;
            _jugadorRepository.Setup(s => s.GetByIdAsync(jugadorId)).ReturnsAsync((Jugador?)null);
            _jugadorRepository.Setup(s => s.DeleteAsync(jugadorId));
            _unitOfWork.Setup(s => s.SaveChangesAsync());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(404);
            result.ErrorMessage.Should().Be("Jugador no encontrado.");
        }

        [Fact]
        public async Task Handle_DebeEjecutarLlamadasEnOrdenLogico()
        {
            // Arrange
            var jugadorId = Guid.NewGuid();
            var command = new DeleteJugadorCommand(jugadorId);
            var jugador = Jugador.Create("Nombre Jugador", Guid.NewGuid()).Value!;
            var ordenEjecucion = new List<string>();
            _jugadorRepository.Setup(s => s.GetByIdAsync(jugadorId))
                .Callback(() => ordenEjecucion.Add("GetByIdAsync"))
                .ReturnsAsync(jugador);
            _jugadorRepository.Setup(s => s.DeleteAsync(jugadorId))
                .Callback(() => ordenEjecucion.Add("DeleteAsync"));
            _unitOfWork.Setup(s => s.SaveChangesAsync())
                .Callback(() => ordenEjecucion.Add("SaveChangesAsync"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            var ordenEsperado = new[] { "GetByIdAsync", "DeleteAsync", "SaveChangesAsync" };
            ordenEjecucion.Should().Equal(ordenEsperado);
        }

        [Fact]
        public async Task Handle_LlamaAMetodosCorrectamenteConParametrosEnviado()
        {
            // Arrange
            var jugadorId = Guid.NewGuid();
            var command = new DeleteJugadorCommand(jugadorId);
            var jugador = Jugador.Create("Nombre Jugador", Guid.NewGuid()).Value!;
            _jugadorRepository.Setup(s => s.GetByIdAsync(jugadorId))
                .ReturnsAsync(jugador);
            _jugadorRepository.Setup(s => s.DeleteAsync(jugadorId));
            _unitOfWork.Setup(s => s.SaveChangesAsync());

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _jugadorRepository.Verify(s => s.GetByIdAsync(It.Is<Guid>(x => x == jugadorId)), Times.Once());
            _jugadorRepository.Verify(s => s.DeleteAsync(It.Is<Guid>(x => x == jugadorId)), Times.Once());
        }
    }
}
