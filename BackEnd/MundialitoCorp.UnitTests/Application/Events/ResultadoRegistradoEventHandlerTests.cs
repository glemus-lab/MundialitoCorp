using MundialitoCorp.Application.Features.Partidos.Events;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Events;
using MundialitoCorp.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Events;

public class ResultadoRegistradoEventHandlerTests
{
    private readonly Mock<IEquipoRepository> _repoMock;
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<ILogger<ResultadoRegistradoEventHandler>> _loggerMock;
    private readonly ResultadoRegistradoEventHandler _handler;

    public ResultadoRegistradoEventHandlerTests()
    {
        _repoMock = new Mock<IEquipoRepository>();
        _uowMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<ResultadoRegistradoEventHandler>>();
        _handler = new ResultadoRegistradoEventHandler(_repoMock.Object, _uowMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_DebeLlamarUPdate_ParaAmbosEquipos()
    {
        // Arrange
        var local = Equipo.Create("Local").Value!;
        var visitante = Equipo.Create("Visitante").Value!;
        var evento = new ResultadoRegistradoEvent(Guid.NewGuid(), local.Id, visitante.Id, 2, 0, new(), new());

        _repoMock.Setup(x => x.GetByIdAsync(local.Id)).ReturnsAsync(local);
        _repoMock.Setup(x => x.GetByIdAsync(visitante.Id)).ReturnsAsync(visitante);

        // Act
        await _handler.Handle(evento, CancellationToken.None);

        // Assert
        local.Puntos.Should().Be(3);
        visitante.Puntos.Should().Be(0);
        _repoMock.Verify(x => x.Update(It.IsAny<Equipo>()), Times.Exactly(2));
        _uowMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
