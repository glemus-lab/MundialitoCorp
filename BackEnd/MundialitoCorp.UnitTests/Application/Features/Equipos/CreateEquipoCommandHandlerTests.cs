using MundialitoCorp.Application.Features.Equipos.Commands.CreateEquipo;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Equipos
{
    public class CreateEquipoCommandHandlerTests
    {
        private readonly Mock<IEquipoRepository> _equipoRepositoryMock;
        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
        private readonly CreateEquipoCommandHandler _handler;
        private readonly Mock<ILogger<CreateEquipoCommandHandler>> _logger;

        public CreateEquipoCommandHandlerTests()
        {
            _equipoRepositoryMock = new Mock<IEquipoRepository>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _logger = new Mock<ILogger<CreateEquipoCommandHandler>>();

            _handler = new CreateEquipoCommandHandler(
                _unitOfWorkMock.Object,
                _equipoRepositoryMock.Object,
                _logger.Object);
        }

        [Fact]
        public async Task Handle_DebeCrearEquipoExitosamente_CuandoElNombreNoExiste()
        {
            // Arrange
            var command = new CreateEquipoCommand("Tech FC");

            _equipoRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeEmpty();

            _equipoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Equipo>()), Times.Once);

            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_DebeRetornarFalla_CuandoElNombreYaExiste()
        {
            // Arrange
            var command = new CreateEquipoCommand("Equipo Duplicado");

            _equipoRepositoryMock.Setup(x => x.ExistsAsync(command.Nombre))
                .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.ErrorMessage.Should().Be("Error de validación.");
            result.Errors[0].PropertyName.Should().Be("Nombre");
            result.Errors[0].Message.Should().Be("Ya existe un equipo registrado con ese nombre.");
            result.Code.Should().Be(409);

            _equipoRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Equipo>()), Times.Never);
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DebeLanzarExcepcion_CuandoElRepositorioFalla()
        {
            // Arrange
            var command = new CreateEquipoCommand("Error Test");
            _equipoRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Error de base de datos"));

            // Act
            Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Error de base de datos");
            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handle_DebeAsignarValoresCorrectosALaEntidad_AntesDeLlamarAlRepositorio()
        {
            // Arrange
            var nombreEsperado = "Equipo 1";
            var command = new CreateEquipoCommand(nombreEsperado);
            Equipo equipoCapturado = null!;

            _equipoRepositoryMock.Setup(x => x.ExistsAsync(It.IsAny<string>()))
                .ReturnsAsync(false);
            _equipoRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Equipo>()))
                .Callback<Equipo>(e => equipoCapturado = e)
                .Returns(Task.CompletedTask);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            equipoCapturado.Should().NotBeNull();
            equipoCapturado.Id.Should().NotBeEmpty();
            equipoCapturado.Nombre.Should().Be(nombreEsperado);
            equipoCapturado.Puntos.Should().Be(0);
            equipoCapturado.PartidosJugados.Should().Be(0);
            equipoCapturado.GolesFavor.Should().Be(0);
            equipoCapturado.GolesContra.Should().Be(0);
            equipoCapturado.DomainEvents.Should().ContainSingle(e =>
                e is MundialitoCorp.Domain.Events.EquipoCreadoEvent);
        }

        [Fact]
        public async Task Handle_DebeEjecutarLlamadasEnOrdenLogico()
        {
            // Arrange
            var command = new CreateEquipoCommand("Equipo 1");
            var ordenEjecucion = new List<string>();
            _equipoRepositoryMock
                .Setup(x => x.ExistsAsync(It.IsAny<string>()))
                .Callback(() => ordenEjecucion.Add("ExistsAsync"))
                .ReturnsAsync(false);
            _equipoRepositoryMock
                .Setup(x => x.AddAsync(It.IsAny<Equipo>()))
                .Callback(() => ordenEjecucion.Add("AddAsync"))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock
                .Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Callback(() => ordenEjecucion.Add("SaveChangesAsync"))
                .ReturnsAsync(1);

            // Act
            await _handler.Handle(command, CancellationToken.None);

            // Assert
            var ordenEsperado = new[] { "ExistsAsync", "AddAsync", "SaveChangesAsync" };
            ordenEjecucion.Should().Equal(ordenEsperado);
        }
    }
}