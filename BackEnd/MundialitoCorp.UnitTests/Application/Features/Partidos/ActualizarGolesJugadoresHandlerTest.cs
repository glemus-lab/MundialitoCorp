using MundialitoCorp.Application.Features.Partidos.Events;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Events;
using MundialitoCorp.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Partidos
{
    public class ActualizarGolesJugadoresHandlerTest
    {
        private readonly ActualizarGolesJugadoresHandler _handler;
        private readonly Mock<IJugadorRepository> _jugadorRepository;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<ILogger<ActualizarGolesJugadoresHandler>> _logger;

        public ActualizarGolesJugadoresHandlerTest()
        {
            _jugadorRepository = new Mock<IJugadorRepository>();
            _unitOfWork = new Mock<IUnitOfWork>();
            _logger = new Mock<ILogger<ActualizarGolesJugadoresHandler>>();
            _handler = new ActualizarGolesJugadoresHandler(_jugadorRepository.Object, _unitOfWork.Object, _logger.Object);
        }

        [Fact]
        public async Task Handle_RegistraGolesAJugadores()
        {
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();

            var jugador1L = Jugador.Create("Jugador 1", equipoIdL).Value!;
            var jugador2L = Jugador.Create("Jugador 2", equipoIdL).Value!;
            var jugador1V = Jugador.Create("Jugador 1", equipoIdV).Value!;

            var listadoGoleadoresL = new List<Guid>() { jugador1L.Id, jugador1L.Id, jugador2L.Id };
            var listaGoleadoresV = new List<Guid>() { jugador1V.Id };

            var registro = new ResultadoRegistradoEvent(Guid.NewGuid(), equipoIdL, equipoIdV, 3, 1, listadoGoleadoresL, listaGoleadoresV);

            _jugadorRepository.Setup(s => s.GetByIdAsync(jugador1L.Id)).ReturnsAsync(jugador1L);
            _jugadorRepository.Setup(s => s.GetByIdAsync(jugador2L.Id)).ReturnsAsync(jugador2L);
            _jugadorRepository.Setup(s => s.GetByIdAsync(jugador1V.Id)).ReturnsAsync(jugador1V);
            _jugadorRepository.Setup(s => s.Update(It.IsAny<Jugador>()));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()));

            // Act
            await _handler.Handle(registro, CancellationToken.None);

            // Assert
            jugador1L.GolesAnotados.Should().Be(2);
            jugador2L.GolesAnotados.Should().Be(1);
            jugador1V.GolesAnotados.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NoLlamaAMetodos_CuandoListadoGoleadoresEsVacio()
        {
            var listadoGoleadoresL = new List<Guid>();
            var listaGoleadoresV = new List<Guid>();

            var registro = new ResultadoRegistradoEvent(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), 3, 1, listadoGoleadoresL, listaGoleadoresV);


            // Act
            await _handler.Handle(registro, CancellationToken.None);

            // Assert
            _jugadorRepository.Verify(s => s.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
            _jugadorRepository.Verify(s => s.Update(It.IsAny<Jugador>()), Times.Never);
            _unitOfWork.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DebeEjecutarLlamadasEnOrdenLogico()
        {
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();

            var jugador1L = Jugador.Create("Jugador 1", equipoIdL).Value!;
            var jugador2L = Jugador.Create("Jugador 2", equipoIdL).Value!;
            var jugador1V = Jugador.Create("Jugador 1", equipoIdV).Value!;

            var listadoGoleadoresL = new List<Guid>() { jugador1L.Id, jugador1L.Id, jugador2L.Id };
            var listaGoleadoresV = new List<Guid>() { jugador1V.Id };

            var registro = new ResultadoRegistradoEvent(Guid.NewGuid(), equipoIdL, equipoIdV, 3, 1, listadoGoleadoresL, listaGoleadoresV);

            var ordenEjecucion = new List<string>();

            _jugadorRepository.Setup(s => s.GetByIdAsync(jugador1L.Id))
                .Callback(() => ordenEjecucion.Add("GetByIdAsyncJugador1L"))
                .ReturnsAsync(jugador1L);
            _jugadorRepository.Setup(s => s.GetByIdAsync(jugador2L.Id))
                .Callback(() => ordenEjecucion.Add("GetByIdAsyncJugador2L"))
                .ReturnsAsync(jugador2L);
            _jugadorRepository.Setup(s => s.GetByIdAsync(jugador1V.Id))
                .Callback(() => ordenEjecucion.Add("GetByIdAsyncJugador1V"))
                .ReturnsAsync(jugador1V);
            _jugadorRepository.Setup(s => s.Update(jugador1L))
                .Callback(() => ordenEjecucion.Add("UpdateJugador1L"));
            _jugadorRepository.Setup(s => s.Update(jugador2L))
                .Callback(() => ordenEjecucion.Add("UpdateJugador2L"));
            _jugadorRepository.Setup(s => s.Update(jugador1V))
                .Callback(() => ordenEjecucion.Add("UpdateJugador1V"));

            // Act
            await _handler.Handle(registro, CancellationToken.None);

            // Assert
            var ordenEsperado = new[] {
                "GetByIdAsyncJugador1L", "UpdateJugador1L",
                "GetByIdAsyncJugador1L", "UpdateJugador1L",
                "GetByIdAsyncJugador2L", "UpdateJugador2L",
                "GetByIdAsyncJugador1V", "UpdateJugador1V" };
            ordenEjecucion.Should().BeEqualTo(ordenEsperado);
            _unitOfWork.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_EjecutarLlamadasConParametrosEnviados()
        {
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();

            var jugador1L = Jugador.Create("Jugador 1", equipoIdL).Value!;
            var jugador2L = Jugador.Create("Jugador 2", equipoIdL).Value!;
            var jugador1V = Jugador.Create("Jugador 1", equipoIdV).Value!;

            var listadoGoleadoresL = new List<Guid>() { jugador1L.Id, jugador1L.Id, jugador2L.Id };
            var listaGoleadoresV = new List<Guid>() { jugador1V.Id };

            var registro = new ResultadoRegistradoEvent(Guid.NewGuid(), equipoIdL, equipoIdV, 3, 1, listadoGoleadoresL, listaGoleadoresV);

            var ordenEjecucion = new List<string>();

            _jugadorRepository.Setup(s => s.GetByIdAsync(jugador1L.Id)).ReturnsAsync(jugador1L);
            _jugadorRepository.Setup(s => s.GetByIdAsync(jugador2L.Id)).ReturnsAsync(jugador2L);
            _jugadorRepository.Setup(s => s.GetByIdAsync(jugador1V.Id)).ReturnsAsync(jugador1V);
            _jugadorRepository.Setup(s => s.Update(jugador1L));
            _jugadorRepository.Setup(s => s.Update(jugador2L));
            _jugadorRepository.Setup(s => s.Update(jugador1V));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()));

            // Act
            await _handler.Handle(registro, CancellationToken.None);

            // Assert
            _jugadorRepository.Verify(s => s.GetByIdAsync(It.Is<Guid>(s => s == jugador1L.Id)), Times.Exactly(2));
            _jugadorRepository.Verify(s => s.GetByIdAsync(It.Is<Guid>(s => s == jugador2L.Id)), Times.Once);
            _jugadorRepository.Verify(s => s.GetByIdAsync(It.Is<Guid>(s => s == jugador1V.Id)), Times.Once);
            _jugadorRepository.Verify(s => s.Update(It.Is<Jugador>(s => s.Id == jugador1L.Id)), Times.Exactly(2));
            _jugadorRepository.Verify(s => s.Update(It.Is<Jugador>(s => s.Id == jugador2L.Id)), Times.Once);
            _jugadorRepository.Verify(s => s.Update(It.Is<Jugador>(s => s.Id == jugador1V.Id)), Times.Once);
            _unitOfWork.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}