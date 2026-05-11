using MundialitoCorp.Application.Features.Equipos.Events;
using MundialitoCorp.Domain.Events;
using Microsoft.Extensions.Logging;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Equipos
{
    public class EquipoCreadoEventHandlerTests
    {
        private readonly Mock<ILogger<EquipoCreadoEventHandler>> _loggerMock;
        private readonly EquipoCreadoEventHandler _handler;

        public EquipoCreadoEventHandlerTests()
        {
            _loggerMock = new Mock<ILogger<EquipoCreadoEventHandler>>();
            _handler = new EquipoCreadoEventHandler(_loggerMock.Object);
        }

        [Fact]
        public async Task Handle_DebeRegistrarLogDeInformacion_CuandoSeRecibeElEvento()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var nombreEquipo = "Capa Application FC";
            var evento = new EquipoCreadoEvent(equipoId, nombreEquipo);

            // Act
            await _handler.Handle(evento, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(nombreEquipo)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_DebeContenerElIdDelEquipoEnElLog()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var evento = new EquipoCreadoEvent(equipoId, "Test Team");

            // Act
            await _handler.Handle(evento, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(equipoId.ToString())),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}