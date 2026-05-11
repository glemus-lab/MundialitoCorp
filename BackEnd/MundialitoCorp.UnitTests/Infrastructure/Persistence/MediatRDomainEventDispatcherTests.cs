using FluentAssertions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.ValueObjects;
using MundialitoCorp.Infrastructure.Logging;
using MundialitoCorp.Infrastructure.Persistence;

namespace MundialitoCorp.UnitTests.Infrastructure.Persistence
{
    public class MediatRDomainEventDispatcherTests
    {
        private readonly Mock<IPublisher> _publisherMock;
        private readonly Mock<EventLogger<MediatRDomainEventDispatcher>> _loggerMock;
        private readonly Mock<ICorrelationIdGenerator> _correlationIdGeneratorMock;
        private readonly ApplicationDbContext _context;

        public MediatRDomainEventDispatcherTests()
        {
            _publisherMock = new Mock<IPublisher>();
            _loggerMock = new Mock<EventLogger<MediatRDomainEventDispatcher>>(new Mock<ILogger<MediatRDomainEventDispatcher>>().Object);
            _correlationIdGeneratorMock = new Mock<ICorrelationIdGenerator>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
        }

        [Fact]
        public async Task DispatchEventsAsync_DebePublicarYLimpiarEventos_CuandoExistenEntidadesConEventos()
        {
            var traceId = "test-trace-id";
            _correlationIdGeneratorMock.Setup(x => x.Get()).Returns(traceId);

            var equipoLocalId = Guid.NewGuid();
            var equipoVisitanteId = Guid.NewGuid();
            var goleadorId = Guid.NewGuid();

            var partido = Partido.Create(equipoLocalId, equipoVisitanteId, DateOnly.FromDateTime(DateTime.Now)).Value!;

            var resultado = Resultado.Create(1, 0).Value!;
            var goleadoresLocal = new List<Guid> { goleadorId };
            var goleadoresVisitante = new List<Guid>();

            partido.RegistrarResultado(resultado, goleadoresLocal, goleadoresVisitante);

            _context.Entry(partido).State = EntityState.Added;

            var dispatcher = new MediatRDomainEventDispatcher(
                _publisherMock.Object,
                _loggerMock.Object,
                _correlationIdGeneratorMock.Object);

            await dispatcher.DispatchEventsAsync(_context);

            _publisherMock.Verify(
                x => x.Publish(It.IsAny<object>(), It.IsAny<CancellationToken>()),
                Times.AtLeastOnce);

            _loggerMock.Verify(
                x => x.LogDomainEvent(It.IsAny<INotification>(), traceId),
                Times.AtLeastOnce);

            partido.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public async Task DispatchEventsAsync_NoDebeHacerNada_SiNoHayEventos()
        {
            // Arrange
            _correlationIdGeneratorMock.Setup(x => x.Get()).Returns("id");
            var dispatcher = new MediatRDomainEventDispatcher(
                _publisherMock.Object,
                _loggerMock.Object,
                _correlationIdGeneratorMock.Object);

            // Act
            await dispatcher.DispatchEventsAsync(_context);

            // Assert
            _publisherMock.Verify(x => x.Publish(It.IsAny<INotification>(), default), Times.Never);
        }
    }
}