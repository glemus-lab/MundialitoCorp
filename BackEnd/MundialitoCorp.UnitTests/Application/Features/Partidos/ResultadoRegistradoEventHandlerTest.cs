using MundialitoCorp.Application.Features.Partidos.Events;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Events;
using MundialitoCorp.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Partidos
{
    public class ResultadoRegistradoEventHandlerTest
    {
        private readonly ResultadoRegistradoEventHandler _handler;
        private readonly Mock<IEquipoRepository> _equipoRepository;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<ILogger<ResultadoRegistradoEventHandler>> _logger;

        public ResultadoRegistradoEventHandlerTest()
        {
            _equipoRepository = new();
            _unitOfWork = new();
            _logger = new();
            _handler = new(_equipoRepository.Object, _unitOfWork.Object, _logger.Object);
        }

        [Fact]
        public async Task Handlet_ActualizaCorrectamenteLasEstadisticasDeLosEquipos()
        {
            // Arrange
            var equipoL = Equipo.Create("EquipoL").Value!;
            var equipoV = Equipo.Create("EquipoV").Value!;
            var equipoIdL = equipoL.Id;
            var equipoIdV = equipoV.Id;

            equipoL.ActualizarEstadisticas(1, 1);
            equipoV.ActualizarEstadisticas(1, 1);

            var golesL = 2;
            var golesV = 3;
            var notification = new ResultadoRegistradoEvent(Guid.NewGuid(), equipoIdL, equipoIdV, golesL, golesV, [], []);

            _equipoRepository.Setup(s => s.GetByIdAsync(equipoIdL)).ReturnsAsync(equipoL);
            _equipoRepository.Setup(s => s.GetByIdAsync(equipoIdV)).ReturnsAsync(equipoV);
            _equipoRepository.Setup(s => s.Update(It.IsAny<Equipo>()));

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            equipoL.GolesFavor.Should().Be(3);
            equipoL.GolesContra.Should().Be(4);
            equipoV.GolesFavor.Should().Be(4);
            equipoV.GolesContra.Should().Be(3);
        }

        [Fact]
        public async Task Handlet_LlamaAUpdateDeCadaEquipo_ConValoresActualizados()
        {
            // Arrange
            var equipoL = Equipo.Create("EquipoL").Value!;
            var equipoV = Equipo.Create("EquipoV").Value!;
            var equipoIdL = equipoL.Id;
            var equipoIdV = equipoV.Id;

            var golesL = 2;
            var golesV = 3;
            var notification = new ResultadoRegistradoEvent(Guid.NewGuid(), equipoIdL, equipoIdV, golesL, golesV, [], []);

            _equipoRepository.Setup(s => s.GetByIdAsync(equipoIdL)).ReturnsAsync(equipoL);
            _equipoRepository.Setup(s => s.GetByIdAsync(equipoIdV)).ReturnsAsync(equipoV);
            _equipoRepository.Setup(s => s.Update(It.IsAny<Equipo>()));

            // Act
            await _handler.Handle(notification, CancellationToken.None);

            // Assert
            _equipoRepository.Verify(s => s.Update(It.IsAny<Equipo>()), Times.Exactly(2));
            _equipoRepository.Verify(s => s.Update(It.Is<Equipo>(x => 
                        x.Id == equipoIdL &&
                        x.GolesFavor == 2 &&
                        x.GolesContra == 3)), Times.Once());
            _equipoRepository.Verify(s => s.Update(It.Is<Equipo>(x => 
                        x.Id == equipoIdV &&
                        x.GolesFavor == 3 &&
                        x.GolesContra == 2)), Times.Once());
        }

        [Fact]
        public async Task Handlet_LanzaException_CuandoEquipoLNoExiste()
        {
            // Arrange
            var equipoL = Equipo.Create("EquipoL").Value!;
            var equipoV = Equipo.Create("EquipoV").Value!;
            var equipoIdL = equipoL.Id;
            var equipoIdV = equipoV.Id;

            var golesL = 0;
            var golesV = 0;
            var notification = new ResultadoRegistradoEvent(Guid.NewGuid(), equipoIdL, equipoIdV, golesL, golesV, [], []);

            _equipoRepository.Setup(s => s.GetByIdAsync(equipoIdL)).ReturnsAsync((Equipo?)null);
            _equipoRepository.Setup(s => s.GetByIdAsync(equipoIdV)).ReturnsAsync(equipoV);
            _equipoRepository.Setup(s => s.Update(It.IsAny<Equipo>()));

            // Act
            Func<Task> act = () => _handler.Handle(notification, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
            exception.Message.Should().Be("No se pueden actualizar estadísticas: Uno de los equipos no existe.");
            _equipoRepository.Verify(s => s.Update(It.IsAny<Equipo>()), Times.Never());
        }

        [Fact]
        public async Task Handlet_LanzaException_CuandoEquipoVNoExiste()
        {
            // Arrange
            var equipoL = Equipo.Create("EquipoL").Value!;
            var equipoV = Equipo.Create("EquipoV").Value!;
            var equipoIdL = equipoL.Id;
            var equipoIdV = equipoV.Id;

            var golesL = 0;
            var golesV = 0;
            var notification = new ResultadoRegistradoEvent(Guid.NewGuid(), equipoIdL, equipoIdV, golesL, golesV, [], []);

            _equipoRepository.Setup(s => s.GetByIdAsync(equipoIdL)).ReturnsAsync(equipoL);
            _equipoRepository.Setup(s => s.GetByIdAsync(equipoIdV)).ReturnsAsync((Equipo?)null);
            _equipoRepository.Setup(s => s.Update(It.IsAny<Equipo>()));

            // Act
            Func<Task> act = () => _handler.Handle(notification, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
            exception.Message.Should().Be("No se pueden actualizar estadísticas: Uno de los equipos no existe.");
            _equipoRepository.Verify(s => s.Update(It.IsAny<Equipo>()), Times.Never());
        }

        [Fact]
        public async Task Handlet_LanzaException_CuandoGolesLEsNegativo()
        {
            // Arrange
            var equipoL = Equipo.Create("EquipoL").Value!;
            var equipoV = Equipo.Create("EquipoV").Value!;
            var equipoIdL = equipoL.Id;
            var equipoIdV = equipoV.Id;

            var golesL = -1;
            var golesV = 0;
            var notification = new ResultadoRegistradoEvent(Guid.NewGuid(), equipoIdL, equipoIdV, golesL, golesV, [], []);

            _equipoRepository.Setup(s => s.GetByIdAsync(equipoIdL)).ReturnsAsync(equipoL);
            _equipoRepository.Setup(s => s.GetByIdAsync(equipoIdV)).ReturnsAsync(equipoV);
            _equipoRepository.Setup(s => s.Update(It.IsAny<Equipo>()));

            // Act
            Func<Task> act = () => _handler.Handle(notification, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
            exception.Message.Should().Be("No se pueden actualizar estadísticas: Los goles no pueden ser negativos.");
            _equipoRepository.Verify(s => s.Update(It.IsAny<Equipo>()), Times.Never());
        }

        [Fact]
        public async Task Handlet_LanzaException_CuandoGolesVEsNegativo()
        {
            // Arrange
            var equipoL = Equipo.Create("EquipoL").Value!;
            var equipoV = Equipo.Create("EquipoV").Value!;
            var equipoIdL = equipoL.Id;
            var equipoIdV = equipoV.Id;

            var golesL = 0;
            var golesV = -1;
            var notification = new ResultadoRegistradoEvent(Guid.NewGuid(), equipoIdL, equipoIdV, golesL, golesV, [], []);

            _equipoRepository.Setup(s => s.GetByIdAsync(equipoIdL)).ReturnsAsync(equipoL);
            _equipoRepository.Setup(s => s.GetByIdAsync(equipoIdV)).ReturnsAsync(equipoV);
            _equipoRepository.Setup(s => s.Update(It.IsAny<Equipo>()));

            // Act
            Func<Task> act = () => _handler.Handle(notification, CancellationToken.None);

            // Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(act);
            exception.Message.Should().Be("No se pueden actualizar estadísticas: Los goles no pueden ser negativos.");
            _equipoRepository.Verify(s => s.Update(It.IsAny<Equipo>()), Times.Never());
        }
    }
}