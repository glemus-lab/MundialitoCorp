using Dapper;
using FluentAssertions;
using MundialitoCorp.Infrastructure.Persistence.Queries;
using MundialitoCorp.UnitTests.Infrastructure.Persistence.Queries;

namespace MundialitoCorp.Tests.Infrastructure.Queries
{
    public class PartidoQueryServiceTests : QueryServiceTestBase
    {
        private readonly PartidoQueryService _queryService;

        public PartidoQueryServiceTests()
        {
            _queryService = new PartidoQueryService(DapperContextMock.Object);
        }

        [Fact]
        public async Task ExisteConflictoFechaAsync_DebeRetornarTrue_CuandoElEquipoYaJuegaEseDiaComoLocal()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);

            await Connection.ExecuteAsync(@"
                INSERT INTO Partidos (Id, EquipoLocalId, EquipoVisitanteId, Fecha, EstaFinalizado) 
                VALUES (@Id, @EId, @OtroId, @Fecha, 0)",
                new { Id = Guid.NewGuid(), EId = equipoId, OtroId = Guid.NewGuid(), Fecha = fecha });

            // Act
            var resultado = await _queryService.ExisteConflictoFechaAsync(equipoId, fecha);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task ExisteConflictoFechaAsync_DebeRetornarTrue_CuandoElEquipoYaJuegaEseDiaComoVisitante()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);

            await Connection.ExecuteAsync(@"
                INSERT INTO Partidos (Id, EquipoLocalId, EquipoVisitanteId, Fecha, EstaFinalizado) 
                VALUES (@Id, @OtroId, @EId, @Fecha, 0)",
                new { Id = Guid.NewGuid(), OtroId = Guid.NewGuid(), EId = equipoId, Fecha = fecha });

            // Act
            var resultado = await _queryService.ExisteConflictoFechaAsync(equipoId, fecha);

            // Assert
            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task ExisteConflictoFechaAsync_DebeRetornarFalse_CuandoNoHayPartidosProgramados()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);

            // Act
            var resultado = await _queryService.ExisteConflictoFechaAsync(equipoId, fecha);

            // Assert
            resultado.Should().BeFalse();
        }

        [Fact]
        public async Task GetPartidosPendientesAsync_DebeRetornarListaDePartidosNoFinalizados()
        {
            // Arrange
            var localId = Guid.NewGuid();
            var visitanteId = Guid.NewGuid();

            await Connection.ExecuteAsync("INSERT INTO Equipos (Id, Nombre) VALUES (@Id, 'Local FC')", new { Id = localId });
            await Connection.ExecuteAsync("INSERT INTO Equipos (Id, Nombre) VALUES (@Id, 'Visitante FC')", new { Id = visitanteId });

            await Connection.ExecuteAsync(@"
                INSERT INTO Partidos (Id, EquipoLocalId, EquipoVisitanteId, EstaFinalizado, Fecha) VALUES 
                (@Id1, @LId, @VId, 0, '2024-01-01'),
                (@Id2, @LId, @VId, 1, '2024-01-02')",
                new { Id1 = Guid.NewGuid(), Id2 = Guid.NewGuid(), LId = localId, VId = visitanteId });

            // Act
            var resultados = (await _queryService.GetPartidosPendientesAsync()).ToList();

            // Assert
            resultados.Should().NotBeEmpty();
            resultados.Should().HaveCount(1);
            resultados.All(p => p.EstaFinalizado == false).Should().BeTrue();
            resultados.First().Local.Should().Be("Local FC");
        }

        [Fact]
        public async Task GetPartidosPendientesAsync_DebeRetornarVacio_CuandoTodosLosPartidosEstanFinalizados()
        {
            // Arrange
            var localId = Guid.NewGuid();
            var visitanteId = Guid.NewGuid();

            await Connection.ExecuteAsync("INSERT INTO Equipos (Id, Nombre) VALUES (@Id, 'Local FC')", new { Id = localId });
            await Connection.ExecuteAsync("INSERT INTO Equipos (Id, Nombre) VALUES (@Id, 'Visitante FC')", new { Id = visitanteId });

            await Connection.ExecuteAsync(@"
                INSERT INTO Partidos (Id, EquipoLocalId, EquipoVisitanteId, EstaFinalizado, Fecha) 
                VALUES (@Id, @LId, @VId, 1, '2024-01-01')",
                new { Id = Guid.NewGuid(), LId = localId, VId = visitanteId });

            // Act
            var resultados = await _queryService.GetPartidosPendientesAsync();

            // Assert
            resultados.Should().BeEmpty();
        }
    }
}