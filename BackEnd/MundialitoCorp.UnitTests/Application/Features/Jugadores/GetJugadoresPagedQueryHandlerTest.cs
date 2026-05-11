using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Features.Jugadores.Queries.GetJugadoresPaged;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MundialitoCorp.Infrastructure.Persistence.Queries;
using FluentAssertions;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Jugadores
{
    public class GetJugadoresPagedQueryHandlerTest
    {
        private readonly GetJugadoresPagedQueryHandler _handler;
        private readonly Mock<IJugadorQueryService> _jugadorQueryService;

        public GetJugadoresPagedQueryHandlerTest()
        {
            _jugadorQueryService = new Mock<IJugadorQueryService>();
            _handler = new GetJugadoresPagedQueryHandler(_jugadorQueryService.Object);
        }

        [Fact]
        public async Task Handle_RegresaResultSuccess_ConPagedListDeJugadorReadModel()
        {
            // Arrange
            var listado = new PagedList<JugadorReadModel>(new List<JugadorReadModel>(), 0, 1, 5);
            var query = new GetJugadoresPagedQuery(1, 5, string.Empty, Guid.NewGuid());
            _jugadorQueryService.Setup(s => s.GetJugadoresPagedAsync(1, 5, string.Empty, query.EquipoId))
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
        public async Task Handle_LlamaAGetJugadoresPagedAsyncConParametrosCorrectamente()
        {
            // Arrange
            var listado = new PagedList<JugadorReadModel>(new List<JugadorReadModel>(), 0, 1, 5);
            var query = new GetJugadoresPagedQuery(1, 5, "Nombre Equipo", Guid.NewGuid());
            _jugadorQueryService.Setup(s => s.GetJugadoresPagedAsync(1, 5, string.Empty, Guid.Empty))
                .ReturnsAsync(listado);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _jugadorQueryService.Verify(s => s.GetJugadoresPagedAsync(
                        It.Is<int>(x => x == query.PageNumber),
                        It.Is<int>(x => x == query.PageSize),
                        It.Is<string>(x => x == query.NombreFilter),
                        It.Is<Guid>(x => x == query.EquipoId)));
        }
    }
}