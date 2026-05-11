using MundialitoCorp.Application.Features.Partidos.Queries.GetPartidosPendientes;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using FluentAssertions;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Partidos
{
    public class GetPartidosPendientesQueryHandlerTest
    {
        private readonly GetPartidosPendientesQueryHandler _handler;
        private readonly Mock<IPartidoQueryService> _queryService;

        public GetPartidosPendientesQueryHandlerTest()
        {
            _queryService = new();
            _handler = new(_queryService.Object);
        }

        [Fact]
        public async Task Handler_RegresaResultSuccess_ConListadoDePartidoReadModel()
        {
            // Arrange
            var query = new GetPartidosPendientesQuery();
            var listado = new List<PartidoReadModel>();
            _queryService.Setup(s => s.GetPartidosPendientesAsync()).ReturnsAsync(listado);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsType<Result<List<PartidoReadModel>>>(result);
            result.IsSuccess.Should().BeTrue();
            result.Code.Should().Be(200);
            result.Value.Should().BeEqualTo(listado);
        }

        [Fact]
        public async Task Handler_LlamaAGetPartidosPendientesAsync()
        {
            // Arrange
            var query = new GetPartidosPendientesQuery();
            var listado = new List<PartidoReadModel>();
            _queryService.Setup(s => s.GetPartidosPendientesAsync()).ReturnsAsync(listado);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _queryService.Verify(s => s.GetPartidosPendientesAsync(), Times.Once());
        }
    }
}
