using MundialitoCorp.Application.Features.Jugadores.Commands.CreateJugador;
using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Jugadores
{
    public class CreateJugadorCommandHandlerTest
    {
        private readonly CreateJugadorCommandHandler _handler;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IEquipoRepository> _equipoRepository;
        private readonly Mock<ILogger<CreateJugadorCommandHandler>> _logger;

        public CreateJugadorCommandHandlerTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _equipoRepository = new Mock<IEquipoRepository>();
            _logger = new Mock<ILogger<CreateJugadorCommandHandler>>();
            _handler = new CreateJugadorCommandHandler(_unitOfWork.Object, _equipoRepository.Object, _logger.Object);
        }

        [Fact]
        public async Task Handler_RegresaResultConGuidDelJugadorCreado_CuandoJugadorEsValido()
        {
            // Arrange
            var equipo = Equipo.Create("Equipo Test").Value!;
            var command = new CreateJugadorCommand("Jugador Nuevo", equipo.Id);
            _equipoRepository.Setup(x => x.GetByIdAsync(equipo.Id)).ReturnsAsync(equipo);
            _unitOfWork.Setup(x => x.SaveChangesAsync(CancellationToken.None));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result<Guid>>(result);
            result.IsSuccess.Should().BeTrue();
            result.Code.Should().Be(201);
            result.Value.Should().Be(equipo.Jugadores[0].Id);
        }

        [Fact]
        public async Task Handler_AgregaAJugadorALFinalDelListadoDeEquipo_CuandoJugadorEsValido()
        {
            // Arrange
            var equipo = Equipo.Create("Equipo Test").Value!;
            equipo.AgregarJugador("Jugador Existente");
            var command = new CreateJugadorCommand("Jugador Nuevo", equipo.Id);
            _equipoRepository.Setup(x => x.GetByIdAsync(equipo.Id)).ReturnsAsync(equipo);
            _unitOfWork.Setup(x => x.SaveChangesAsync(CancellationToken.None));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            equipo.Jugadores.Count.Should().Be(2);
            result.Value.Should().Be(equipo.Jugadores.Last().Id);
        }

        [Fact]
        public async Task Handler_RegresaResultFailure_CuandoEquipoIdNoExiste()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var command = new CreateJugadorCommand("Jugador Nuevo", equipoId);
            _equipoRepository.Setup(x => x.GetByIdAsync(equipoId)).ReturnsAsync((Equipo?)null);
            _unitOfWork.Setup(x => x.SaveChangesAsync(CancellationToken.None));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result<Guid>>(result);
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be("El equipo destino no existe.");
            result.Code.Should().Be(404);
            result.Value.Should().BeEmpty();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Handler_RegresaResultFailure_CuandoNombreDeJugadorNoEsValido(string? nombre)
        {
            // Arrange
            var equipo = Equipo.Create("Equipo Test").Value!;
            var command = new CreateJugadorCommand(nombre!, equipo.Id);
            _equipoRepository.Setup(x => x.GetByIdAsync(equipo.Id)).ReturnsAsync(equipo);
            _unitOfWork.Setup(x => x.SaveChangesAsync(CancellationToken.None));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result<Guid>>(result);
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be("Error de validación.");
            result.Code.Should().Be(422);
            result.Value.Should().BeEmpty();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].PropertyName.Should().Be("Nombre");
            result.Errors[0].Message.Should().Be("El nombre del jugador no puede estar vacío.");
        }

        [Fact]
        public async Task Handler_RegresaResultFailure_CuandoCantidadSuperaLos25JugadoresDeEquipo()
        {
            // Arrange
            var equipo = Equipo.Create("Equipo Test").Value!;
            for(int i = 1; i <= 25; i++)
            {
                equipo.AgregarJugador($"Jugador {i}");
            }
            var command = new CreateJugadorCommand("Jugador Nuevo", equipo.Id);
            _equipoRepository.Setup(x => x.GetByIdAsync(equipo.Id)).ReturnsAsync(equipo);
            _unitOfWork.Setup(x => x.SaveChangesAsync(CancellationToken.None));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result<Guid>>(result);
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be("El equipo ya tiene el máximo de jugadores.");
            result.Code.Should().Be(422);
            result.Value.Should().BeEmpty();
        }

        [Fact]
        public async Task Handler_RegresaResultFailure_CuandoNombreJugadorSuperaLos150Caracteres()
        {
            // Arrange
            var equipo = Equipo.Create("Equipo Test").Value!;
            var nombre = new string('A', 151);
            var command = new CreateJugadorCommand(nombre, equipo.Id);
            _equipoRepository.Setup(x => x.GetByIdAsync(equipo.Id)).ReturnsAsync(equipo);
            _unitOfWork.Setup(x => x.SaveChangesAsync(CancellationToken.None));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result<Guid>>(result);
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be("Error de validación.");
            result.Code.Should().Be(422);
            result.Value.Should().BeEmpty();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].PropertyName.Should().Be("Nombre");
            result.Errors[0].Message.Should().Be("El nombre no puede contener más de 150 caracteres.");
        }

        [Fact]
        public async Task Handle_DebeEjecutarLlamadasEnOrdenLogico()
        {
            // Arrange
            var equipo = Equipo.Create("Equipo Test").Value!;
            var command = new CreateJugadorCommand("Jugador Nuevo", equipo.Id);
            var ordenEjecucion = new List<string>();
            _equipoRepository.Setup(x => x.GetByIdAsync(equipo.Id))
                .Callback(() => ordenEjecucion.Add("GetByIdAsyncEquipo"))
                .ReturnsAsync(equipo);
            _unitOfWork.Setup(x => x.SaveChangesAsync(CancellationToken.None))
                .Callback(() => ordenEjecucion.Add("SaveChangesAsync"));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            var ordenEsperado = new[] { "GetByIdAsyncEquipo", "SaveChangesAsync" };
            ordenEjecucion.Should().Equal(ordenEsperado);
        }

        [Fact]
        public async Task Handle_AgregaJugadorAEquipo_AntesDeLlamarASaveChangesAsync()
        {
            // Arrange
            var equipo = Equipo.Create("Equipo Test").Value!;
            var command = new CreateJugadorCommand("Jugador Nuevo", equipo.Id);
            var agregoJugador = false;
            _equipoRepository.Setup(x => x.GetByIdAsync(equipo.Id))
                .ReturnsAsync(equipo);
            _unitOfWork.Setup(x => x.SaveChangesAsync(CancellationToken.None))
                .Callback(() => agregoJugador = equipo.Jugadores.Count == 1);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            agregoJugador.Should().BeTrue();
        }
    }
}