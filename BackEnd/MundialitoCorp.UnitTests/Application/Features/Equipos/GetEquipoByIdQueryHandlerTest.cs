using MundialitoCorp.Application.Features.Equipos.Queries.GetEquipoById;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using FluentAssertions;
using Moq;

namespace MundialitoCorp.UnitTests.Application.Features.Equipos
{
    public class GetEquipoByIdQueryHandlerTest
    {
        private readonly GetEquipoByIdQueryHandler _handler;
        private readonly Mock<IEquipoQueryService> _queryService;

        public GetEquipoByIdQueryHandlerTest()
        {
            _queryService = new Mock<IEquipoQueryService>();
            _handler = new GetEquipoByIdQueryHandler(_queryService.Object);
        }

        [Fact]
        public async Task Handle_RegresaResultSuccessConEquipoEnValue()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var query = new GetEquipoByIdQuery(id);
            var equipo = new EquipoReadModel(id, "Equipo Test");

            _queryService.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(equipo);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Code.Should().Be(200);
            result.Value.Should().Be(equipo);
        }

        [Fact]
        public async Task Handle_RegresaResultFailure_CuandoEquipoNoExiste()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var query = new GetEquipoByIdQuery(id);

            _queryService.Setup(x => x.GetByIdAsync(id)).ReturnsAsync((EquipoReadModel?)null);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(404);
            result.Value.Should().BeNull();
            result.ErrorMessage.Should().Be("El equipo solicitado no existe.");
        }

        [Fact]
        public async Task Handle_LlamaAGetByIdAsyncDeServicio_ConParametrosCorrectos()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            var query = new GetEquipoByIdQuery(id);
            var equipo = new EquipoReadModel(id, "Equipo Test");

            _queryService.Setup(x => x.GetByIdAsync(id)).ReturnsAsync(equipo);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            _queryService.Verify(x => x.GetByIdAsync(id), Times.Once());
        }
    }
}
