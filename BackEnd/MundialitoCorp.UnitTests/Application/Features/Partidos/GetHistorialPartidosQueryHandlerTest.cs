using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Features.Partidos.Queries.GetHistorialPartidos;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using FluentAssertions;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Partidos
{
    public class GetHistorialPartidosQueryHandlerTest
    {
        private readonly GetHistorialPartidosQueryHandler _handler;
        private readonly Mock<IPartidoQueryService> _queryService;

        public GetHistorialPartidosQueryHandlerTest()
        {
            _queryService = new Mock<IPartidoQueryService>();
            _handler = new GetHistorialPartidosQueryHandler(_queryService.Object);
        }

        [Fact]
        public async Task Handle_RegresaREsultSuccess_ConPagedListDePartidoReadModel()
        {
            // Arrange
            var query = new GetHistorialPartidosQuery(1, 5);
            var listado = new PagedList<PartidoReadModel>([], 0, 1, 5);
            _queryService.Setup(s => s.GetHistorialPartidosAsync(1, 5)).ReturnsAsync(listado);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsType<Result<PagedList<PartidoReadModel>>>(result);
            result.IsSuccess.Should().BeTrue();
            result.Code.Should().Be(200);
            result.Value.Should().Be(listado);
        }

        [Fact]
        public async Task Handle_LlamaAGetHistorialPartidosAsyncConParametrosEnviado()
        {
            // Arrange
            var query = new GetHistorialPartidosQuery(1, 5);
            var listado = new PagedList<PartidoReadModel>([], 0, 1, 5);
            _queryService.Setup(s => s.GetHistorialPartidosAsync(1, 5)).ReturnsAsync(listado);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _queryService.Verify(s => s.GetHistorialPartidosAsync(It.Is<int>(x => x == query.PageNumber), It.Is<int>(x => x == query.PageSize)), Times.Once());
        }
    }
}
