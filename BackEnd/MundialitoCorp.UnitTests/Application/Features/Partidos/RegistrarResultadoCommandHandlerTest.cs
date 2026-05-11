using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MundialitoCorp.Application.Features.Partidos.Commands.RegistrarResultado;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Repositories;
using MundialitoCorp.Domain.ValueObjects;

namespace MundialitoCorp.UnitTests.Application.Features.Partidos
{
    public class RegistrarResultadoCommandHandlerTest
    {
        private readonly RegistrarResultadoCommandHandler _handler;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IPartidoRepository> _partidoRepository;
        private readonly Mock<IJugadorQueryService> _jugadorQueryService;
        private readonly Mock<ILogger<RegistrarResultadoCommandHandler>> _logger;

        public RegistrarResultadoCommandHandlerTest()
        {
            _unitOfWork = new();
            _partidoRepository = new();
            _jugadorQueryService = new();
            _logger = new();
            _handler = new(_unitOfWork.Object, _partidoRepository.Object, _jugadorQueryService.Object, _logger.Object);
        }

        [Fact]
        public async Task Handler_RegresaResultSucces_CuandoSeRegistranResultadosCorrectamente()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);
            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;

            var jugador1L = Jugador.Create("Jugador 1 Local", equipoIdL).Value!;
            var jugador1V = Jugador.Create("Jugador 1 Visitante", equipoIdV).Value!;
            var jugador2V = Jugador.Create("Jugador 2 Visitante", equipoIdV).Value!;

            var command = new RegistrarResultadoCommand(partido.Id, 1, 2, [jugador1L.Id], [jugador1V.Id, jugador2V.Id]);

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _jugadorQueryService.Setup(s => s.ExistenTodosLosJugadoresAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(true);
            _partidoRepository.Setup(s => s.Update(partido));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsSuccess.Should().BeTrue();
            result.Code.Should().Be(200);
        }

        [Fact]
        public async Task Handler_LlamaASaveChangesAntesQueCommit()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);
            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;

            var command = new RegistrarResultadoCommand(partido.Id, 0, 0, [], []);
            var ordenEjecucion = new List<string>();

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _jugadorQueryService.Setup(s => s.ExistenTodosLosJugadoresAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(true);
            _partidoRepository.Setup(s => s.Update(partido));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Callback(() => ordenEjecucion.Add("SaveChangesAsync"));
            _unitOfWork.Setup(s => s.CommitAsync())
                .Callback(() => ordenEjecucion.Add("CommitAsync"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            var ordenEsperado = new[] { "SaveChangesAsync", "CommitAsync" };
            ordenEjecucion.Should().BeEqualTo(ordenEsperado);
        }

        [Fact]
        public async Task Handler_LlamaARollbackAsync_CuandoOcurreUnException()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);
            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;

            var jugador1L = Jugador.Create("Jugador 1 Local", equipoIdL).Value!;
            var jugador1V = Jugador.Create("Jugador 1 Visitante", equipoIdV).Value!;
            var jugador2V = Jugador.Create("Jugador 2 Visitante", equipoIdV).Value!;

            var command = new RegistrarResultadoCommand(partido.Id, 1, 2, [jugador1L.Id], [jugador1V.Id, jugador2V.Id]);

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _jugadorQueryService.Setup(s => s.ExistenTodosLosJugadoresAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(true);
            _partidoRepository.Setup(s => s.Update(partido));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error en base de datos."));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _unitOfWork.Verify(s => s.RollbackAsync(), Times.Once());
        }

        [Fact]
        public async Task Handler_RegresaResultFailure_CuandoOcurreUnException()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);
            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;

            var command = new RegistrarResultadoCommand(partido.Id, 0, 0, [], []);

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _jugadorQueryService.Setup(s => s.ExistenTodosLosJugadoresAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(true);
            _partidoRepository.Setup(s => s.Update(partido));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error en base de datos."));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(500);
            result.ErrorMessage.Should().Be("Error interno al procesar la transacción.");
        }

        [Fact]
        public async Task Handler_NoLlamaACommitAsync_CuandoOcurreUnException()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);
            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;

            var jugador1L = Jugador.Create("Jugador 1 Local", equipoIdL).Value!;
            var jugador1V = Jugador.Create("Jugador 1 Visitante", equipoIdV).Value!;
            var jugador2V = Jugador.Create("Jugador 2 Visitante", equipoIdV).Value!;

            var command = new RegistrarResultadoCommand(partido.Id, 1, 2, [jugador1L.Id], [jugador1V.Id, jugador2V.Id]);

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _jugadorQueryService.Setup(s => s.ExistenTodosLosJugadoresAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(true);
            _partidoRepository.Setup(s => s.Update(partido));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Error en base de datos."));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _unitOfWork.Verify(s => s.CommitAsync(), Times.Never());
        }

        [Fact]
        public async Task Handle_DebeAsignarValoresCorrectosALaEntidad_AntesDeLlamarAlRepositorio()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);
            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;

            var jugador1L = Jugador.Create("Jugador 1 Local", equipoIdL).Value!;
            var jugador1V = Jugador.Create("Jugador 1 Visitante", equipoIdV).Value!;
            var jugador2V = Jugador.Create("Jugador 2 Visitante", equipoIdV).Value!;

            var command = new RegistrarResultadoCommand(partido.Id, 1, 2, [jugador1L.Id], [jugador1V.Id, jugador2V.Id]);

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _jugadorQueryService.Setup(s => s.ExistenTodosLosJugadoresAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(true);
            _partidoRepository.Setup(s => s.Update(partido));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _partidoRepository.Verify(s => s.Update(It.Is<Partido>(x =>
                            x.GolesLocal == 1 &&
                            x.GolesVisitante == 2 &&
                            x.EstaFinalizado &&
                            x.GolesDetalle.Count == 3 &&
                            x.GolesDetalle.Any(g => g.Id != Guid.Empty && g.PartidoId == partido.Id && g.JugadorId == jugador1L.Id) &&
                            x.GolesDetalle.Any(g => g.Id != Guid.Empty && g.PartidoId == partido.Id && g.JugadorId == jugador1V.Id) &&
                            x.GolesDetalle.Any(g => g.Id != Guid.Empty && g.PartidoId == partido.Id && g.JugadorId == jugador2V.Id)
                        )), Times.Once());
        }

        [Fact]
        public async Task Handle_LlamaAUpdate_AntesDeSaveChangesAsync()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);
            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;

            var jugador1L = Jugador.Create("Jugador 1 Local", equipoIdL).Value!;
            var jugador1V = Jugador.Create("Jugador 1 Visitante", equipoIdV).Value!;
            var jugador2V = Jugador.Create("Jugador 2 Visitante", equipoIdV).Value!;

            var command = new RegistrarResultadoCommand(partido.Id, 1, 2, [jugador1L.Id], [jugador1V.Id, jugador2V.Id]);

            var ordenEjecucion = new List<string>();

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _jugadorQueryService.Setup(s => s.ExistenTodosLosJugadoresAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(true);
            _partidoRepository.Setup(s => s.Update(partido))
                .Callback(() => ordenEjecucion.Add("Update"));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Callback(() => ordenEjecucion.Add("SaveChangesAsync"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            var ordenEsperado = new[] { "Update", "SaveChangesAsync" };
            ordenEjecucion.Should().BeEqualTo(ordenEsperado);
        }

        [Fact]
        public async Task Handler_RegresaResultFailure_CuandoPartidoNoExiste()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);
            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;

            var command = new RegistrarResultadoCommand(partido.Id, 0, 0, [], []);

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync((Partido?)null);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(404);
            result.ErrorMessage.Should().Be("Partido no encontrado.");
        }
        [Fact]
        public async Task Handler_RegresaResultFailure_CuandoAlgunJugadorDelListadoNoExiste()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);
            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;

            var jugador1L = Jugador.Create("Jugador 1 Local", equipoIdL).Value!;
            var jugador1V = Jugador.Create("Jugador 1 Visitante", equipoIdV).Value!;
            var jugador2V = Jugador.Create("Jugador 2 Visitante", equipoIdV).Value!;

            var command = new RegistrarResultadoCommand(partido.Id, 1, 2, [jugador1L.Id], [jugador1V.Id, jugador2V.Id]);

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _jugadorQueryService.Setup(s => s.ExistenTodosLosJugadoresAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(false);
            _partidoRepository.Setup(s => s.Update(partido));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(422);
            result.ErrorMessage.Should().Be("Uno o más goleadores no son válidos o no existen.");
        }

        [Fact]
        public async Task Handler_PasaComoParametroAExistenTodosLosJugadoresAsync_ListadoCompletoDeJugadores()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);
            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;

            var jugador1L = Jugador.Create("Jugador 1 Local", equipoIdL).Value!;
            var jugador1V = Jugador.Create("Jugador 1 Visitante", equipoIdV).Value!;
            var jugador2V = Jugador.Create("Jugador 2 Visitante", equipoIdV).Value!;

            var command = new RegistrarResultadoCommand(partido.Id, 1, 2, [jugador1L.Id], [jugador1V.Id, jugador2V.Id]);

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _jugadorQueryService.Setup(s => s.ExistenTodosLosJugadoresAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(true);
            _partidoRepository.Setup(s => s.Update(partido));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _jugadorQueryService.Verify(s => s.ExistenTodosLosJugadoresAsync(It.Is<List<Guid>>(s => s.Count == 3)), Times.Once());
        }

        [Fact]
        public async Task Handler_RegresaResultFailure_CuandoPartidoYaHaSidoFinalizado()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);
            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;
            var resultado = Resultado.Create(0, 0).Value!;
            partido.RegistrarResultado(resultado, [], []);

            var command = new RegistrarResultadoCommand(partido.Id, 0, 0, [], []);

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _jugadorQueryService.Setup(s => s.ExistenTodosLosJugadoresAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(422);
            result.ErrorMessage.Should().Be("El partido ya ha sido finalizado previamente.");
        }

        [Fact]
        public async Task Handler_RegresaResultFailure_CuandoFechaEsAFuturo()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(2));
            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;

            var command = new RegistrarResultadoCommand(partido.Id, 0, 0, [], []);

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _jugadorQueryService.Setup(s => s.ExistenTodosLosJugadoresAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(422);
            result.ErrorMessage.Should().Be("No se pueden registrar resultado de un partido que no se ha disputado.");
        }

        [Fact]
        public async Task Handler_RegresaResultFailure_CuandoGolesSonNegativos()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(2));
            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;

            var command = new RegistrarResultadoCommand(partido.Id, -1, -1, [], []);

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _jugadorQueryService.Setup(s => s.ExistenTodosLosJugadoresAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(422);
            result.ErrorMessage.Should().Be("Error de validación.");
            result.Errors.Count.Should().Be(2);
            result.Errors[0].PropertyName.Should().Be("GolesLocal");
            result.Errors[0].Message.Should().Be("Los goles del local no pueden ser negativos.");
            result.Errors[1].PropertyName.Should().Be("GolesVisitante");
            result.Errors[1].Message.Should().Be("Los goles del visitante no pueden ser negativos.");
        }

        [Fact]
        public async Task Handler_RegresaResultFailure_CuandoGolesNoCoincideConListadoDeGoleadores()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(2));
            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;

            var command = new RegistrarResultadoCommand(partido.Id, 1, 2, [Guid.NewGuid(), Guid.NewGuid()], [Guid.NewGuid()]);

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _jugadorQueryService.Setup(s => s.ExistenTodosLosJugadoresAsync(It.IsAny<List<Guid>>()))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(422);
            result.ErrorMessage.Should().Be("Error de validación.");
            result.Errors.Count.Should().Be(2);
            result.Errors[0].PropertyName.Should().Be("GolesLocal");
            result.Errors[0].Message.Should().Be("Los goles del local deben coincidir con los goleadores");
            result.Errors[1].PropertyName.Should().Be("GolesVisitante");
            result.Errors[1].Message.Should().Be("Los goles del visitante deben coincidir con los goleadores.");
        }
    }
}