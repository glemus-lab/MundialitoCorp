using MundialitoCorp.Application.Features.Partidos.Commands.CreatePartido;
using MundialitoCorp.Domain.Repositories;
using MundialitoCorp.Domain.Entities;
using Moq;
using FluentAssertions;
using MundialitoCorp.Application.Interfaces;
using Microsoft.Extensions.Logging;
using MundialitoCorp.Domain.Common;

namespace MundialitoCorp.UnitTests.Application.Features.Partidos;

public class CreatePartidoCommandHandlerTests
{
    private readonly Mock<IPartidoRepository> _partidoRepoMock;
    private readonly Mock<IPartidoQueryService> _partidoQueryMock;
    private readonly Mock<IEquipoRepository> _equipoRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWork;
    private readonly CreatePartidoCommandHandler _handler;
    private readonly Mock<ILogger<CreatePartidoCommandHandler>> _logger;

    public CreatePartidoCommandHandlerTests()
    {
        _partidoRepoMock = new Mock<IPartidoRepository>();
        _partidoQueryMock = new Mock<IPartidoQueryService>();
        _equipoRepoMock = new Mock<IEquipoRepository>();
        _unitOfWork = new Mock<IUnitOfWork>();
        _logger = new Mock<ILogger<CreatePartidoCommandHandler>>();
        _handler = new CreatePartidoCommandHandler(_unitOfWork.Object, _partidoRepoMock.Object, _equipoRepoMock.Object, _partidoQueryMock.Object, _logger.Object);
    }

    [Fact]
    public async Task Handle_RegresaResultSuccessConGuid_CuandoCrearPartidoCorrectamente()
    {
        // Arrange
        var localId = Guid.NewGuid();
        var visitanteId = Guid.NewGuid();
        var fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var command = new CreatePartidoCommand(localId, visitanteId, fecha);
        Partido partidoCreado = null!;

        _equipoRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(Equipo.Create("Equipo").Value!);
        _partidoQueryMock.Setup(x => x.ExisteConflictoFechaAsync(localId, fecha)).ReturnsAsync(false);
        _partidoRepoMock.Setup(s => s.AddAsync(It.IsAny<Partido>()))
            .Callback<Partido>(s => partidoCreado = s);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsType<Result<Guid>>(result);
        result.IsSuccess.Should().BeTrue();
        result.Code.Should().Be(201);
        result.Value.Should().NotBeEmpty();
        result.Value.Should().Be(partidoCreado.Id);
        partidoCreado.EquipoLocalId.Should().Be(localId);
        partidoCreado.EquipoVisitanteId.Should().Be(visitanteId);
        partidoCreado.Fecha.Should().Be(fecha);
        partidoCreado.EstaFinalizado.Should().BeFalse();
        partidoCreado.GolesLocal.Should().BeNull();
        partidoCreado.GolesVisitante.Should().BeNull();
    }

    [Fact]
    public async Task Handle_RegresaResultFailureConGuid_CuandoNoExistenLosEquipos()
    {
        // Arrange
        var localId = Guid.NewGuid();
        var visitanteId = Guid.NewGuid();
        var fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var command = new CreatePartidoCommand(localId, visitanteId, fecha);
        var equipo = Equipo.Create("Equipo").Value!;

        _equipoRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Equipo?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsType<Result<Guid>>(result);
        result.IsFailure.Should().BeTrue();
        result.Code.Should().Be(422);
        result.ErrorMessage.Should().Be("No se pudo crear el partido.");
        result.Errors.Count.Should().Be(2);
        result.Errors[0].PropertyName.Should().Be("EquipoLocalId");
        result.Errors[0].Message.Should().Be("El equipo no está registrado.");
        result.Errors[1].PropertyName.Should().Be("EquipoVisitanteId");
        result.Errors[1].Message.Should().Be("El equipo no está registrado.");
    }

    [Fact]
    public async Task Handle_RegresaResultFailureConGuid_CuandoExisteConflictoDeFechas()
    {
        // Arrange
        var localId = Guid.NewGuid();
        var visitanteId = Guid.NewGuid();
        var fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var command = new CreatePartidoCommand(localId, visitanteId, fecha);
        var equipo = Equipo.Create("Equipo").Value!;

        _equipoRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(equipo);
        _partidoQueryMock.Setup(s => s.ExisteConflictoFechaAsync(It.IsAny<Guid>(), fecha)).ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsType<Result<Guid>>(result);
        result.IsFailure.Should().BeTrue();
        result.Code.Should().Be(422);
        result.ErrorMessage.Should().Be("No se pudo crear el partido.");
        result.Errors.Count.Should().Be(2);
        result.Errors[0].PropertyName.Should().Be("EquipoLocalId");
        result.Errors[0].Message.Should().Be("El equipo ya tiene un partido programado en esta fecha.");
        result.Errors[1].PropertyName.Should().Be("EquipoVisitanteId");
        result.Errors[1].Message.Should().Be("El equipo ya tiene un partido programado en esta fecha.");
    }

    [Fact]
    public async Task Handle_RegresaResultFailureConGuid_CuandoLosEquiposSonElMismo()
    {
        // Arrange
        var localId = Guid.NewGuid();
        var visitanteId = localId;
        var fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
        var command = new CreatePartidoCommand(localId, visitanteId, fecha);
        var equipo = Equipo.Create("Equipo").Value!;

        _equipoRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(equipo);
        _partidoQueryMock.Setup(s => s.ExisteConflictoFechaAsync(It.IsAny<Guid>(), fecha)).ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.IsType<Result<Guid>>(result);
        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeEmpty();
        result.Code.Should().Be(422);
        result.ErrorMessage.Should().Be("Los equipos rivales no puede ser el mismo.");
    }

    [Fact]
    public async Task Handle_DebeEjecutarLosLlamadosEnOrdenLogico()
    {
        // Arrange
        var localId = Guid.NewGuid();
        var visitanteId = Guid.NewGuid();
        var fecha = DateOnly.FromDateTime(DateTime.Now);
        var command = new CreatePartidoCommand(localId, visitanteId, fecha);
        var equipo = Equipo.Create("Equipo").Value!;
        var ordenEjecucion = new List<string>();

        _equipoRepoMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .Callback<Guid>(s => ordenEjecucion.Add($"GetByIdAsync{s}"))
            .ReturnsAsync(equipo);
        _partidoQueryMock.Setup(s => s.ExisteConflictoFechaAsync(It.IsAny<Guid>(), fecha))
            .Callback<Guid, DateOnly>((g, f) => ordenEjecucion.Add($"ExisteConflictoFechaAsync{g}"))
            .ReturnsAsync(false);
        _partidoRepoMock.Setup(s => s.AddAsync(It.IsAny<Partido>()))
            .Callback(() => ordenEjecucion.Add("AddAsync"));
        _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Callback(() => ordenEjecucion.Add("SaveChangesAsync"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var ordenEsperado = new[] { 
            $"GetByIdAsync{localId}", $"GetByIdAsync{visitanteId}", 
            $"ExisteConflictoFechaAsync{localId}", $"ExisteConflictoFechaAsync{visitanteId}",
            "AddAsync", "SaveChangesAsync"
        };
        ordenEjecucion.Should().Equal(ordenEsperado);
    }

    [Fact]
    public async Task Handle_RealizaLosLlamadosCorrectamenteConLosParametrosEnviados()
    {
        // Arrange
        var equipoL = Equipo.Create("Equipo Local").Value!;
        var equipoV = Equipo.Create("Equipo Visitante").Value!;
        var localId = equipoL.Id;
        var visitanteId = equipoV.Id;
        var fecha = DateOnly.FromDateTime(DateTime.Now);
        var command = new CreatePartidoCommand(localId, visitanteId, fecha);

        _equipoRepoMock.Setup(x => x.GetByIdAsync(localId)).ReturnsAsync(equipoL);
        _equipoRepoMock.Setup(x => x.GetByIdAsync(visitanteId)).ReturnsAsync(equipoV);
        _partidoQueryMock.Setup(s => s.ExisteConflictoFechaAsync(localId, fecha)).ReturnsAsync(false);
        _partidoQueryMock.Setup(s => s.ExisteConflictoFechaAsync(visitanteId, fecha)).ReturnsAsync(false);
        _partidoRepoMock.Setup(s => s.AddAsync(It.IsAny<Partido>()));
        _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _equipoRepoMock.Verify(s => s.GetByIdAsync(It.Is<Guid>(x => x == localId)), Times.Once());
        _equipoRepoMock.Verify(s => s.GetByIdAsync(It.Is<Guid>(x => x == visitanteId)), Times.Once());
        _partidoQueryMock.Verify(s => s.ExisteConflictoFechaAsync(It.Is<Guid>(x => x == localId), It.Is<DateOnly>(x => x == fecha)), Times.Once());
        _partidoQueryMock.Verify(s => s.ExisteConflictoFechaAsync(It.Is<Guid>(x => x == visitanteId), It.Is<DateOnly>(x => x == fecha)), Times.Once());
        _partidoRepoMock.Verify(s => s.AddAsync(It.IsAny<Partido>()), Times.Once());
        _unitOfWork.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once());
    }
}