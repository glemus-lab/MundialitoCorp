using ControlTorneoFootball.Application.Features.Partidos.Queries.GetPartidoById;
using FluentAssertions;
using Moq;
using MundialitoCorp.Application.Features.Partidos.Queries.GetPartidoById;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Entities;

namespace MundialitoCorp.UnitTests.Application.Features.Partidos
{
    public class GetPartidoByIdQueryHandlerTest
    {
        private readonly GetPartidoByIdQueryHandler _handler;
        private readonly Mock<IPartidoQueryService> _partidoQueryService;

        public GetPartidoByIdQueryHandlerTest()
        {
            _partidoQueryService = new();
            _handler = new(_partidoQueryService.Object);
        }

        [Fact]
        public async Task Handler_RegresaResultSucces_CuandoPartidoExiste()
        {
            // Arrange
            var query = new GetPartidoByIdQuery(Guid.NewGuid());
            var partidoModel = new PartidoDetalleReadModel(Guid.NewGuid(), "EquipoL", Guid.NewGuid(), "EquipoV", 0, 0, DateOnly.FromDateTime(DateTime.Now), []);
            _partidoQueryService.Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(partidoModel);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Arrange
            Assert.IsType<Result<PartidoDetalleReadModel>>(result);
            result.IsSuccess.Should().BeTrue();
            result.Code.Should().Be(200);
            result.Value.Should().Be(partidoModel);
        }

        [Fact]
        public async Task Handler_LlamaAGetByIdAsync_ConParametrosCorrectos()
        {
            // Arrange
            var query = new GetPartidoByIdQuery(Guid.NewGuid());
            var partidoModel = new PartidoDetalleReadModel(Guid.NewGuid(), "EquipoL", Guid.NewGuid(), "EquipoV", 0, 0, DateOnly.FromDateTime(DateTime.Now), []);
            _partidoQueryService.Setup(s => s.GetByIdAsync(query.Id))
                .ReturnsAsync(partidoModel);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Arrange
            _partidoQueryService.Verify(s => s.GetByIdAsync(It.Is<Guid>(x => x == query.Id)), Times.Once());
        }

        [Fact]
        public async Task Handler_RegresaResultFailure_CuandoPartidoNoExiste()
        {
            // Arrange
            var query = new GetPartidoByIdQuery(Guid.NewGuid());
            _partidoQueryService.Setup(s => s.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((PartidoDetalleReadModel?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Arrange
            Assert.IsType<Result<PartidoDetalleReadModel>>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(404);
            result.ErrorMessage.Should().Be("El partido solicitado no existe.");
        }
    }
}
