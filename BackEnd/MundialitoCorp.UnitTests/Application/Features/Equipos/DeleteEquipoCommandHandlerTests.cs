using Castle.Core.Logging;
using MundialitoCorp.Application.Features.Equipos.Commands.DeleteEquipo;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Equipos
{
    public class DeleteEquipoCommandHandlerTests
    {
        private readonly Mock<IEquipoRepository> _equipoRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly DeleteEquipoCommandHandler _handler;
        private readonly Mock<ILogger<DeleteEquipoCommandHandler>> _logger;

        public DeleteEquipoCommandHandlerTests()
        {
            _equipoRepositoryMock = new Mock<IEquipoRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _logger = new Mock<ILogger<DeleteEquipoCommandHandler>>();
            _handler = new DeleteEquipoCommandHandler(_unitOfWorkMock.Object, _equipoRepositoryMock.Object, _logger.Object);
        }

        [Fact]
        public async Task Handle_DebeEliminarEquipo_CuandoNoTienePartidosJugados()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var equipo = Equipo.Create("Equipo a Eliminar").Value;
            var command = new DeleteEquipoCommand(equipoId);

            _equipoRepositoryMock.Setup(x => x.GetByIdAsync(equipoId))
                .ReturnsAsync(equipo);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            _equipoRepositoryMock.Verify(x => x.DeleteAsync(equipoId), Times.Once);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DebeRetornarFalla_CuandoElEquipoNoExiste()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            _equipoRepositoryMock.Setup(x => x.GetByIdAsync(equipoId))
                .ReturnsAsync((Equipo?)null);

            // Act
            var result = await _handler.Handle(new DeleteEquipoCommand(equipoId), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Code.Should().Be(404);
            _equipoRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DebeRetornarFalla_CuandoElEquipoYaTienePartidosJugados()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var equipo = Equipo.Create("Equipo con Historia").Value!;
            equipo.ActualizarEstadisticas(1, 0);

            _equipoRepositoryMock.Setup(x => x.GetByIdAsync(equipoId))
                .ReturnsAsync(equipo);

            // Act
            var result = await _handler.Handle(new DeleteEquipoCommand(equipoId), CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Code.Should().Be(422);
            _equipoRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Guid>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DebeEjecutarLlamadasEnOrdenLogico()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var equipo = Equipo.Create("Equipo Test").Value!;
            var command = new DeleteEquipoCommand(equipoId);

            var ordenEjecucion = new List<string>();
            _equipoRepositoryMock.Setup(x => x.GetByIdAsync(equipoId))
                .Callback(() => ordenEjecucion.Add("GetByIdAsync"))
                .ReturnsAsync(equipo);

            _equipoRepositoryMock.Setup(x => x.DeleteAsync(equipoId))
                .Callback(() => ordenEjecucion.Add("DeleteAsync"))
                .Returns(Task.CompletedTask);

            _unitOfWorkMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Callback(() => ordenEjecucion.Add("SaveChangesAsync"))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var ordenEsperado = new[] { "GetByIdAsync", "DeleteAsync", "SaveChangesAsync" };
            ordenEjecucion.Should().Equal(ordenEsperado);
        }
    }
}