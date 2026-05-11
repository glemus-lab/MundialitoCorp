using MundialitoCorp.Application.Features.Jugadores.Commands.UpdateJugador;
using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Jugadores
{
    public class UpdateJugadorCommandHandlerTest
    {
        private readonly UpdateJugadorCommandHandler _handler;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IJugadorRepository> _jugadorRepository;
        private readonly Mock<ILogger<UpdateJugadorCommandHandler>> _logger;

        public UpdateJugadorCommandHandlerTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _jugadorRepository = new Mock<IJugadorRepository>();
            _logger = new Mock<ILogger<UpdateJugadorCommandHandler>>();
            _handler = new UpdateJugadorCommandHandler(_unitOfWork.Object, _jugadorRepository.Object, _logger.Object);
        }

        [Fact]
        public async Task Handle_RegresaResultSucces_CuandoActualizaCorrectamente()
        {
            // Arrange
            var jugador = Jugador.Create("Nombre Jugador", Guid.NewGuid()).Value!;
            var command = new UpdateJugadorCommand(jugador.Id, "Nombre Nuevo");
            _jugadorRepository.Setup(s => s.GetByIdAsync(command.Id))
                .ReturnsAsync(jugador);
            _jugadorRepository.Setup(s => s.Update(jugador));
            _unitOfWork.Setup(s => s.SaveChangesAsync(CancellationToken.None));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsSuccess.Should().BeTrue();
            result.Code.Should().Be(200);
        }

        [Fact]
        public async Task Handle_RegresaResultFailure_CuandoJugadorNoExiste()
        {
            // Arrange
            var jugador = Jugador.Create("Nombre Jugador", Guid.NewGuid()).Value!;
            var command = new UpdateJugadorCommand(jugador.Id, "Nombre Nuevo");
            _jugadorRepository.Setup(s => s.GetByIdAsync(command.Id))
                .ReturnsAsync((Jugador?)null);
            _jugadorRepository.Setup(s => s.Update(jugador));
            _unitOfWork.Setup(s => s.SaveChangesAsync(CancellationToken.None));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(404);
            result.ErrorMessage.Should().Be("Jugador no encontrado.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Handle_RegresaResultFailure_CuandoNombreJugadorEsVacio(string? nombre)
        {
            // Arrange
            var jugador = Jugador.Create("Nombre Jugador", Guid.NewGuid()).Value!;
            var command = new UpdateJugadorCommand(jugador.Id, nombre!);
            _jugadorRepository.Setup(s => s.GetByIdAsync(command.Id))
                .ReturnsAsync(jugador);
            _jugadorRepository.Setup(s => s.Update(jugador));
            _unitOfWork.Setup(s => s.SaveChangesAsync(CancellationToken.None));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(422);
            result.ErrorMessage.Should().Be("Error al actualizar el nombre del jugador.");
            result.Errors[0].PropertyName.Should().Be("Nombre");
            result.Errors[0].Message.Should().Be("El nombre del jugador no puede estar vacío.");
        }

        [Fact]
        public async Task Handle_RegresaResultFailure_CuandoNombreJugadorSuperaLos150Caracteres()
        {
            // Arrange
            var jugador = Jugador.Create("Nombre Jugador", Guid.NewGuid()).Value!;
            var nombre = new string('A', 151);
            var command = new UpdateJugadorCommand(jugador.Id, nombre);
            _jugadorRepository.Setup(s => s.GetByIdAsync(command.Id))
                .ReturnsAsync(jugador);
            _jugadorRepository.Setup(s => s.Update(jugador));
            _unitOfWork.Setup(s => s.SaveChangesAsync(CancellationToken.None));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(422);
            result.ErrorMessage.Should().Be("Error al actualizar el nombre del jugador.");
            result.Errors[0].PropertyName.Should().Be("Nombre");
            result.Errors[0].Message.Should().Be("El nombre no puede contener más de 150 caracteres.");
        }

        [Fact]
        public async Task Handle_DebeEjecutarLlamadasEnOrdenLogico()
        {
            // Arrange
            var jugador = Jugador.Create("Nombre Jugador", Guid.NewGuid()).Value!;
            var command = new UpdateJugadorCommand(jugador.Id, "Nuevo Nombre");
            var ordenEjecucion = new List<string>();
            _jugadorRepository.Setup(s => s.GetByIdAsync(command.Id))
                .Callback(() => ordenEjecucion.Add("GetByIdAsync"))
                .ReturnsAsync(jugador);
            _jugadorRepository.Setup(s => s.Update(jugador))
                .Callback(() => ordenEjecucion.Add("Update"));
            _unitOfWork.Setup(s => s.SaveChangesAsync(CancellationToken.None))
                .Callback(() => ordenEjecucion.Add("SaveChangesAsync"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            var ordenEsperada = new List<string>() { "GetByIdAsync", "Update", "SaveChangesAsync" };
            ordenEjecucion.Should().Equal(ordenEsperada);
        }

        [Fact]
        public async Task Handle_ActualizaNombreJugador_AntesDeLlamarAUpdate()
        {
            // Arrange
            var jugador = Jugador.Create("Nombre Jugador", Guid.NewGuid()).Value!;
            var command = new UpdateJugadorCommand(jugador.Id, "Nuevo Nombre");
            var modificaJugador = false;
            _jugadorRepository.Setup(s => s.GetByIdAsync(command.Id))
                .ReturnsAsync(jugador);
            _jugadorRepository.Setup(s => s.Update(jugador))
                .Callback<Jugador>(s => modificaJugador = s.Nombre == "Nuevo Nombre");
            _unitOfWork.Setup(s => s.SaveChangesAsync(CancellationToken.None));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            modificaJugador.Should().BeTrue();
        }
    }
}