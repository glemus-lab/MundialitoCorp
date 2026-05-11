using Dapper;
using FluentAssertions;
using MundialitoCorp.Infrastructure.Persistence.Queries;
using MundialitoCorp.UnitTests.Infrastructure.Persistence.Queries;

namespace MundialitoCorp.Tests.Infrastructure.Queries
{
    public class EquipoQueryServiceTests : QueryServiceTestBase
    {
        private readonly EquipoQueryService _queryService;

        public EquipoQueryServiceTests()
        {
            _queryService = new EquipoQueryService(DapperContextMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarEquipo_CuandoIdExiste()
        {
            var equipoId = Guid.NewGuid();
            await Connection.ExecuteAsync("INSERT INTO Equipos (Id, Nombre) VALUES (@Id, @Nombre)",
                new { Id = equipoId.ToString(), Nombre = "Real Madrid" });

            var resultado = await _queryService.GetByIdAsync(equipoId);

            resultado.Should().NotBeNull();
            resultado!.Nombre.Should().Be("Real Madrid");
            resultado.Id.Should().Be(equipoId);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNull_CuandoIdNoExiste()
        {
            var resultado = await _queryService.GetByIdAsync(Guid.NewGuid());

            resultado.Should().BeNull();
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarListaVacia_CuandoNoHayDatos()
        {
            var resultado = await _queryService.GetAllAsync();

            resultado.Should().BeEmpty();
        }

        [Fact]
        public async Task GetAllAsync_DebeRetornarEquiposOrdenadosAlfabeticamente()
        {
            await Connection.ExecuteAsync("INSERT INTO Equipos (Id, Nombre) VALUES (@Id, @Nombre)", new[] {
                new { Id = Guid.NewGuid().ToString(), Nombre = "Zaragoza" },
                new { Id = Guid.NewGuid().ToString(), Nombre = "Alaves" }
            });

            var resultado = (await _queryService.GetAllAsync()).ToList();

            resultado.Should().HaveCount(2);
            resultado.ElementAt(0).Nombre.Should().Be("Alaves");
            resultado.ElementAt(1).Nombre.Should().Be("Zaragoza");
        }
    }
}