using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Features.Equipos.Queries.GetEquiposPaged;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using FluentAssertions;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Equipos
{
    public class GetEquiposPagedQueryHandlerTests
    {
        private readonly Mock<IEquipoQueryService> _queryServiceMock;
        private readonly GetEquiposPagedQueryHandler _handler;

        public GetEquiposPagedQueryHandlerTests()
        {
            _queryServiceMock = new Mock<IEquipoQueryService>();
            _handler = new GetEquiposPagedQueryHandler(_queryServiceMock.Object);
        }

        [Fact]
        public async Task Handle_DebeLlamarAlServicioConParametrosCorrectos()
        {
            // Arrange
            var query = new GetEquiposPagedQuery(
                PageNumber: 1,
                PageSize: 10,
                SortBy: "Nombre",
                SortDirection: "ASC",
                Filter: "Tech");

            var pagedList = new PagedList<EquipoPagedReadModel>(new List<EquipoPagedReadModel>(), 0, 1, 10);

            _queryServiceMock.Setup(x => x.GetEquiposPagedAsync(
                query.PageNumber,
                query.PageSize,
                query.SortBy,
                query.SortDirection,
                query.Filter))
                .ReturnsAsync(pagedList);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Value?.Data.Should().BeEmpty();
            _queryServiceMock.Verify(x => x.GetEquiposPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Once);
        }
    }
}