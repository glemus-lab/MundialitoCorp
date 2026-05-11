using Castle.Core.Logging;
using MundialitoCorp.Application.Features.Equipos.Commands.UpdateEquipo;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Equipos
{
    public class UpdateEquipoCommandHandlerTests
    {
        private readonly Mock<IEquipoRepository> _equipoRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly UpdateEquipoCommandHandler _handler;
        private readonly Mock<ILogger<UpdateEquipoCommandHandler>> _logger;

        public UpdateEquipoCommandHandlerTests()
        {
            _equipoRepositoryMock = new Mock<IEquipoRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _logger = new Mock<ILogger<UpdateEquipoCommandHandler>>();
            _handler = new UpdateEquipoCommandHandler(_unitOfWorkMock.Object, _equipoRepositoryMock.Object, _logger.Object);
        }

        [Fact]
        public async Task Handle_DebeActualizarNombre_CuandoElNombreEsNuevoYNoExiste()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var equipo = Equipo.Create("Nombre Antiguo").Value!;
            var command = new UpdateEquipoCommand(equipoId, "Nombre Nuevo");

            _equipoRepositoryMock.Setup(x => x.GetByIdAsync(equipoId))
                .ReturnsAsync(equipo);

            _equipoRepositoryMock.Setup(x => x.ExistsAsync("Nombre Nuevo"))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            equipo.Nombre.Should().Be("Nombre Nuevo");

            _equipoRepositoryMock.Verify(x => x.Update(equipo), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DebePermitirActualizar_CuandoElNombreEsElMismoQueYaTiene()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var nombreActual = "Mismo Nombre";
            var equipo = Equipo.Create(nombreActual).Value;
            var command = new UpdateEquipoCommand(equipoId, nombreActual);

            _equipoRepositoryMock.Setup(x => x.GetByIdAsync(equipoId))
                .ReturnsAsync(equipo);

            _equipoRepositoryMock.Setup(x => x.ExistsAsync(nombreActual))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DebeRetornarFalla_CuandoElNombreYaLoTieneOtroEquipo()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var equipo = Equipo.Create("Mi Equipo").Value!;
            var command = new UpdateEquipoCommand(equipoId, "Nombre Ocupado");

            _equipoRepositoryMock.Setup(x => x.GetByIdAsync(equipoId))
                .ReturnsAsync(equipo);

            _equipoRepositoryMock.Setup(x => x.ExistsAsync("Nombre Ocupado"))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Code.Should().Be(409);

            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DebeLanzarExcepcion_CuandoElRepositorioFalla()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var equipo = Equipo.Create("Mi Equipo").Value!;
            var command = new UpdateEquipoCommand(equipoId, "Equipo 1");

            _equipoRepositoryMock.Setup(x => x.GetByIdAsync(equipoId))
                .ReturnsAsync(equipo);

            _equipoRepositoryMock.Setup(x => x.ExistsAsync("Equipo 1"))
                .ThrowsAsync(new Exception("Error de base de datos"));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Error de base de datos");
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DebeRetornarNotFound_CuandoElEquipoNoExiste()
        {
            // Arrange
            _equipoRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Equipo?)null);

            // Act
            var result = await _handler.Handle(new UpdateEquipoCommand(Guid.NewGuid(), "Nombre"), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Code.Should().Be(404);
        }

        [Fact]
        public async Task Handle_DebeAsignarValoresCorrectosALaEntidad_AntesDeLlamarAlRepositorio()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var equipo = Equipo.Create("Equipo 1").Value!;
            equipo.ActualizarEstadisticas(2, 4);
            equipo.ActualizarEstadisticas(1, 0);
            equipo.ActualizarEstadisticas(2, 2);
            var result1 = equipo.AgregarJugador("Jugador 1");
            var result2 = equipo.AgregarJugador("Jugador 2");
            var result3 = equipo.AgregarJugador("Jugador 3");
            var nuevoNombre = "Nombre Nuevo";
            var command = new UpdateEquipoCommand(equipoId, nuevoNombre);

            var ordenEjecucion = new List<string>();
            _equipoRepositoryMock.Setup(x => x.GetByIdAsync(equipoId))
                .ReturnsAsync(equipo);
            _equipoRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _equipoRepositoryMock.Setup(x => x.Update(It.IsAny<Equipo>()));
            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            equipo.Should().NotBeNull();
            equipo.Id.Should().NotBeEmpty();
            equipo.Nombre.Should().Be(nuevoNombre);
            equipo.Puntos.Should().Be(4);
            equipo.PartidosJugados.Should().Be(3);
            equipo.GolesFavor.Should().Be(5);
            equipo.GolesContra.Should().Be(6);
            equipo.Jugadores.Count.Should().Be(3);
            equipo.Jugadores[0].Nombre.Should().Be("Jugador 1");
            equipo.Jugadores[1].Nombre.Should().Be("Jugador 2");
            equipo.Jugadores[2].Nombre.Should().Be("Jugador 3");
            result1.IsSuccess.Should().BeTrue();
            result2.IsSuccess.Should().BeTrue();
            result3.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_DebeEjecutarLlamadasEnOrdenLogico()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var nombreActual = "Equipo 1";
            var equipo = Equipo.Create(nombreActual).Value;
            var command = new UpdateEquipoCommand(equipoId, nombreActual);

            var ordenEjecucion = new List<string>();
            _equipoRepositoryMock.Setup(x => x.GetByIdAsync(equipoId))
                .Callback(() => ordenEjecucion.Add("GetByIdAsync"))
                .ReturnsAsync(equipo);

            _equipoRepositoryMock.Setup(x => x.ExistsAsync(nombreActual))
                .Callback(() => ordenEjecucion.Add("ExistsAsync"))
                .ReturnsAsync(true);

            _equipoRepositoryMock.Setup(x => x.Update(It.IsAny<Equipo>()))
                .Callback(() => ordenEjecucion.Add("Update"));

            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Callback(() => ordenEjecucion.Add("SaveChangesAsync"))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var ordenEsperado = new[] { "GetByIdAsync", "ExistsAsync", "Update", "SaveChangesAsync" };
            ordenEjecucion.Should().Equal(ordenEsperado);
        }
    }
}