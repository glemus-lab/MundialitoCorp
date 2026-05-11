using MundialitoCorp.Application.Features.Equipos.Queries.GetAllEquipos;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using FluentAssertions;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Equipos
{
    public class GetAllEquiposQueryHandlerTest
    {
        private readonly Mock<IEquipoQueryService> _queryService;
        private readonly GetAllEquiposQueryHandler _handler;

        public GetAllEquiposQueryHandlerTest()
        {
            _queryService = new Mock<IEquipoQueryService>();
            _handler = new GetAllEquiposQueryHandler(_queryService.Object);
        }

        [Fact]
        public async Task Handler_RegresaResultSucces_ConIEnumerableDeEquipoReadModel()
        {
            // Arrange
            var listado = Enumerable.Empty<EquipoReadModel>();
            _queryService.Setup(x => x.GetAllAsync()).ReturnsAsync(listado);
            var query = new GetAllEquiposQuery();

            //  Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.IsType<Result<IEnumerable<EquipoReadModel>>>(result);
            result.IsSuccess.Should().BeTrue();
            result.Code.Should().Be(200);
            result.Value.Should().BeEqualTo(listado);
        }

        [Fact]
        public async Task Handler_LlamaAGetAllAsyncDeServicio()
        {
            // Arrange
            var listado = Enumerable.Empty<EquipoReadModel>();
            _queryService.Setup(x => x.GetAllAsync()).ReturnsAsync(listado);
            var query = new GetAllEquiposQuery();

            //  Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            _queryService.Verify(x => x.GetAllAsync(), Times.Once());
        }
    }
}
