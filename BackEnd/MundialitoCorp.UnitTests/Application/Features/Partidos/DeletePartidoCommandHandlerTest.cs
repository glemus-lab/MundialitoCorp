using MundialitoCorp.Application.Features.Partidos.Commands.DeletePartido;
using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Repositories;
using MundialitoCorp.Domain.ValueObjects;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Partidos
{
    public class DeletePartidoCommandHandlerTest
    {
        private readonly DeletePartidoCommandHandler _handle;
        private readonly Mock<IUnitOfWork> _unitOfWork;
        private readonly Mock<IPartidoRepository> _partidoRepository;
        private readonly Mock<ILogger<DeletePartidoCommandHandler>> _logger;

        public DeletePartidoCommandHandlerTest()
        {
            _unitOfWork = new Mock<IUnitOfWork>();
            _partidoRepository = new Mock<IPartidoRepository>();
            _logger = new Mock<ILogger<DeletePartidoCommandHandler>>();
            _handle = new DeletePartidoCommandHandler(_unitOfWork.Object, _partidoRepository.Object, _logger.Object);
        }

        [Fact]
        public async Task Handle_RegresaResultSuccess_CuandoEliminaPartidoCorrectamente()
        {
            // Arrange
            var partido = Partido.Create(Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now)).Value!;

            var command = new DeletePartidoCommand(partido.Id);

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _partidoRepository.Setup(s => s.DeleteAsync(partido.Id));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()));

            // Act
            var result = await _handle.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsSuccess.Should().BeTrue();
            result.Code.Should().Be(204);
        }

        [Fact]
        public async Task Handle_RegresaResultFailure_CuandoPartidoNoExiste()
        {
            // Arrange
            var partido = Partido.Create(Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now)).Value!;

            var command = new DeletePartidoCommand(partido.Id);

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync((Partido?)null);

            // Act
            var result = await _handle.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(404);
            result.ErrorMessage.Should().Be("El partido que intentas eliminar no existe.");
        }

        [Fact]
        public async Task Handle_RegresaResultFailure_CuandoPartidoYaEstaFinalizado()
        {
            // Arrange
            var partido = Partido.Create(Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now)).Value!;
            var resultado = Resultado.Create(0, 0).Value!;
            partido.RegistrarResultado(resultado, [], []);

            var command = new DeletePartidoCommand(partido.Id);

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);

            // Act
            var result = await _handle.Handle(command, CancellationToken.None);

            // Assert
            Assert.IsType<Result>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(422);
            result.ErrorMessage.Should().Be("No se puede eliminar un partido que ya ha finalizado.");
        }

        [Fact]
        public async Task Handle_DebeEjecutarLlamadasEnOrdenLogico()
        {
            // Arrange
            var partido = Partido.Create(Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now)).Value!;
            var resultado = Resultado.Create(0, 0).Value!;

            var command = new DeletePartidoCommand(partido.Id);

            var ordenEjecucion = new List<string>();

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id))
                .Callback(() => ordenEjecucion.Add("GetByIdAsync"))
                .ReturnsAsync(partido);
            _partidoRepository.Setup(s => s.DeleteAsync(partido.Id))
                .Callback(() => ordenEjecucion.Add("DeleteAsync"));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .Callback(() => ordenEjecucion.Add("SaveChangesAsync"));

            // Act
            var result = await _handle.Handle(command, CancellationToken.None);

            // Assert
            var ordenEsperado = new[] { "GetByIdAsync", "DeleteAsync", "SaveChangesAsync" };
            ordenEjecucion.Should().Equal(ordenEsperado);
        }

        [Fact]
        public async Task Handle_RealizaLosLlamadosConLosParametrosCorrespondiente()
        {
            // Arrange
            CancellationToken cancellationToken = CancellationToken.None;
            var partido = Partido.Create(Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now)).Value!;
            var resultado = Resultado.Create(0, 0).Value!;

            var command = new DeletePartidoCommand(partido.Id);

            _partidoRepository.Setup(s => s.GetByIdAsync(partido.Id)).ReturnsAsync(partido);
            _partidoRepository.Setup(s => s.DeleteAsync(partido.Id));
            _unitOfWork.Setup(s => s.SaveChangesAsync(It.IsAny<CancellationToken>()));

            // Act
            var result = await _handle.Handle(command, cancellationToken);

            // Assert
            _partidoRepository.Verify(s => s.GetByIdAsync(It.Is<Guid>(x => x == partido.Id)), Times.Once());
            _partidoRepository.Verify(s => s.DeleteAsync(It.Is<Guid>(x => x == partido.Id)), Times.Once());
            _unitOfWork.Verify(s => s.SaveChangesAsync(It.Is<CancellationToken>(x => x == cancellationToken)), Times.Once());
        }
    }
}
