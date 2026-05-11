using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Features.Jugadores.Queries.GetRankingGoleadores;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using FluentAssertions;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Jugadores
{
    public class GetRankingGoleadoresQueryHandlerTest
    {
        private readonly GetRankingGoleadoresQueryHandler _handler;
        private readonly Mock<IJugadorQueryService> _queryService;

        public GetRankingGoleadoresQueryHandlerTest()
        {
            _queryService = new Mock<IJugadorQueryService>();
            _handler = new GetRankingGoleadoresQueryHandler(_queryService.Object);
        }

        [Fact]
        public async Task Handle_RegresaResultSuccess_ConResultPagedListDeJugadorReadModel()
        {
            // Arrange
            var query = new GetRankingGoleadoresQuery(1, 5);
            var listado = new PagedList<JugadorReadModel>(new List<JugadorReadModel>(), 0, 1, 5);
            _queryService.Setup(s => s.GetRankingGoleadoresAsync(query.PageNumber, query.PageSize))
                .ReturnsAsync(listado);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsType<Result<PagedList<JugadorReadModel>>>(result);
            result.IsSuccess.Should().BeTrue();
            result.Code.Should().Be(200);
            result.Value.Should().Be(listado);
        }

        [Fact]
        public async Task Handle_LlamaAGetRankingGoleadoresAsyncConParametrosCorrectamente()
        {
            // Arrange
            var query = new GetRankingGoleadoresQuery(1, 5);
            var listado = new PagedList<JugadorReadModel>(new List<JugadorReadModel>(), 0, 1, 5);
            _queryService.Setup(s => s.GetRankingGoleadoresAsync(query.PageNumber, query.PageSize))
                .ReturnsAsync(listado);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _queryService.Verify(s => s.GetRankingGoleadoresAsync(
                    It.Is<int>(x => x == query.PageNumber),
                    It.Is<int>(x => x == query.PageSize)));
        }
    }
}