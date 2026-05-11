using MundialitoCorp.Application.Features.Partidos.Commands.UpdateFechaPartido;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Repositories;
using MundialitoCorp.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Partidos
{
    public class UpdateFechaPartidoCommandHandlerTest
    {
        private readonly UpdateFechaPartidoCommandHandler _handler;
        private readonly Mock<IPartidoRepository> _repository;
        private readonly Mock<IPartidoQueryService> _queryService;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<ILogger<UpdateFechaPartidoCommandHandler>> _logger;

        public UpdateFechaPartidoCommandHandlerTest()
        {
            _repository = new();
            _queryService = new();
            _unitOfWork = new();
            _logger = new();
            _handler = new(_repository.Object, _queryService.Object, _unitOfWork.Object, _logger.Object);
        }

        [Fact]
        public async Task Handler_ActualizaFechaDePartidoCorrectamente()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(2));

            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;

            _repository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _queryService.Setup(s => s.ExisteConflictoFechaAsync(equipoIdL, fecha)).ReturnsAsync(false);
            _queryService.Setup(s => s.ExisteConflictoFechaAsync(equipoIdV, fecha)).ReturnsAsync(false);
            _repository.Setup(s => s.Update(It.IsAny<Partido>()));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()));

            var command = new UpdateFechaPartidoCommand(partido.Id, fecha.AddDays(2));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsSuccess.Should().BeTrue();
            result.Code.Should().Be(200);
            partido.Fecha.Should().Be(fecha.AddDays(2));
        }

        [Fact]
        public async Task Handler_LlamaAUpdate_ConFechaModificada()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(2));

            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;

            _repository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _queryService.Setup(s => s.ExisteConflictoFechaAsync(equipoIdL, fecha)).ReturnsAsync(false);
            _queryService.Setup(s => s.ExisteConflictoFechaAsync(equipoIdV, fecha)).ReturnsAsync(false);
            _repository.Setup(s => s.Update(It.IsAny<Partido>()));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()));

            var command = new UpdateFechaPartidoCommand(partido.Id, fecha.AddDays(2));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            _repository.Verify(s => s.Update(It.Is<Partido>(x => x.Fecha == fecha.AddDays(2))), Times.Once());
        }

        [Fact]
        public async Task Handler_LlamaAUpdate_AntesDeSaveChanges()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(2));

            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;
            var ordenEjecucion = new List<string>();

            _repository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _queryService.Setup(s => s.ExisteConflictoFechaAsync(equipoIdL, fecha)).ReturnsAsync(false);
            _queryService.Setup(s => s.ExisteConflictoFechaAsync(equipoIdV, fecha)).ReturnsAsync(false);
            _repository.Setup(s => s.Update(It.IsAny<Partido>()))
                .Callback(() => ordenEjecucion.Add("Update"));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Callback(() => ordenEjecucion.Add("SaveChangesAsync"));

            var command = new UpdateFechaPartidoCommand(partido.Id, fecha.AddDays(2));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            var ordenEsperado = new[] { "Update", "SaveChangesAsync" };
            ordenEjecucion.Should().BeEqualTo(ordenEsperado);
        }

        [Fact]
        public async Task Handler_RegresaResultFailure_CuandoPartidoNoExiste()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(2));

            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;

            _repository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync((Partido?)null);
            _queryService.Setup(s => s.ExisteConflictoFechaAsync(equipoIdL, fecha)).ReturnsAsync(false);
            _queryService.Setup(s => s.ExisteConflictoFechaAsync(equipoIdV, fecha)).ReturnsAsync(false);
            _repository.Setup(s => s.Update(It.IsAny<Partido>()));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()));

            var command = new UpdateFechaPartidoCommand(partido.Id, fecha.AddDays(2));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(404);
            result.ErrorMessage.Should().Be("El partido no existe.");
            _repository.Verify(s => s.Update(It.IsAny<Partido>()), Times.Never);
            _unitOfWork.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handler_RegresaResultFailure_CuandoPartidoYaEstaFinalizado()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);

            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;
            var resultado = Resultado.Create(0, 0).Value!;
            partido.RegistrarResultado(resultado, [], []);

            _repository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _queryService.Setup(s => s.ExisteConflictoFechaAsync(equipoIdL, fecha)).ReturnsAsync(false);
            _queryService.Setup(s => s.ExisteConflictoFechaAsync(equipoIdV, fecha)).ReturnsAsync(false);
            _repository.Setup(s => s.Update(It.IsAny<Partido>()));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()));

            var command = new UpdateFechaPartidoCommand(partido.Id, fecha.AddDays(2));

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(422);
            result.ErrorMessage.Should().Be("No se puede cambiar la fecha de un partido finalizado.");
            _repository.Verify(s => s.Update(It.IsAny<Partido>()), Times.Never);
            _unitOfWork.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handler_RegresaResultFailure_CuandoEquipoLTienenConflictoDeFecha()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);
            var fechaNueva = fecha.AddDays(2);

            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;
            var resultado = Resultado.Create(0, 0).Value!;

            _repository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _queryService.Setup(s => s.ExisteConflictoFechaAsync(equipoIdL, fechaNueva)).ReturnsAsync(true);
            _queryService.Setup(s => s.ExisteConflictoFechaAsync(equipoIdV, fechaNueva)).ReturnsAsync(false);
            _repository.Setup(s => s.Update(It.IsAny<Partido>()));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()));

            var command = new UpdateFechaPartidoCommand(partido.Id, fechaNueva);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(422);
            result.ErrorMessage.Should().Be("Uno de los equipos ya tiene un compromiso en esa fecha.");
            _repository.Verify(s => s.Update(It.IsAny<Partido>()), Times.Never);
            _unitOfWork.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task Handler_RegresaResultFailure_CuandoEquipoVTienenConflictoDeFecha()
        {
            // Arrange
            var equipoIdL = Guid.NewGuid();
            var equipoIdV = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);
            var fechaNueva = fecha.AddDays(2);

            var partido = Partido.Create(equipoIdL, equipoIdV, fecha).Value!;
            var resultado = Resultado.Create(0, 0).Value!;

            _repository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _queryService.Setup(s => s.ExisteConflictoFechaAsync(equipoIdL, fechaNueva)).ReturnsAsync(false);
            _queryService.Setup(s => s.ExisteConflictoFechaAsync(equipoIdV, fechaNueva)).ReturnsAsync(true);
            _repository.Setup(s => s.Update(It.IsAny<Partido>()));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()));

            var command = new UpdateFechaPartidoCommand(partido.Id, fechaNueva);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(422);
            result.ErrorMessage.Should().Be("Uno de los equipos ya tiene un compromiso en esa fecha.");
            _repository.Verify(s => s.Update(It.IsAny<Partido>()), Times.Never);
            _unitOfWork.Verify(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}