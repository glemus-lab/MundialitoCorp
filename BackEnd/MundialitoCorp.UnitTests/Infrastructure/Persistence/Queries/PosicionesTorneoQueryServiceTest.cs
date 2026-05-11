using Dapper;
using FluentAssertions;
using MundialitoCorp.Infrastructure.Persistence.Queries;

namespace MundialitoCorp.UnitTests.Infrastructure.Persistence.Queries
{
    public class PosicionesTorneoQueryServiceTests : QueryServiceTestBase
    {
        private readonly PosicionesTorneoQueryService _queryService;

        public PosicionesTorneoQueryServiceTests()
        {
            _queryService = new PosicionesTorneoQueryService(DapperContextMock.Object);
        }

        [Fact]
        public async Task GetTablaPosicionesAsync_DebeRetornarEquiposOrdenadosPorPuntosYDiferencia()
        {
            // Arrange
            var equipo1 = Guid.NewGuid();
            var equipo2 = Guid.NewGuid();
            var equipo3 = Guid.NewGuid();
            await Connection.ExecuteAsync(@"
                INSERT INTO Equipos (Id, Nombre, Puntos, DiferenciaGoles, GolesFavor, GolesContra, PartidosJugados, PartidosGanados, PartidosPerdidos, PartidosEmpatados) 
                VALUES 
                (@Id1, 'Equipo B', 6, 2, 5, 3, 2, 2, 0, 0),
                (@Id2, 'Equipo A', 6, 5, 8, 3, 2, 2, 0, 0),
                (@Id3, 'Equipo C', 3, 0, 2, 2, 2, 1, 1, 0)",
                new { Id1 = equipo1, Id2 = equipo2, Id3 = equipo3 });

            // Act
            var resultado = (await _queryService.GetTablaPosicionesAsync()).ToList();

            // Assert
            resultado.Should().HaveCount(3);

            resultado[0].Nombre.Should().Be("Equipo A");
            resultado[1].Nombre.Should().Be("Equipo B");
            resultado[2].Nombre.Should().Be("Equipo C");
        }

        [Fact]
        public async Task GetTablaPosicionesAsync_DebeRetornarListaVacia_CuandoNoHayEquipos()
        {
            // Act
            var resultado = await _queryService.GetTablaPosicionesAsync();

            // Assert
            resultado.Should().BeEmpty();
        }

        [Fact]
        public async Task GetTablaPosicionesAsync_DebeMapearTodosLosCamposCorrectamente()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            await Connection.ExecuteAsync(@"
                INSERT INTO Equipos (Id, Nombre, PartidosJugados, PartidosGanados, PartidosPerdidos, PartidosEmpatados, Puntos, GolesFavor, GolesContra, DiferenciaGoles) 
                VALUES (@Id, 'Test FC', 5, 3, 1, 1, 10, 12, 5, 7)",
                new { Id = equipoId });

            // Act
            var resultado = (await _queryService.GetTablaPosicionesAsync()).First();

            // Assert
            resultado.Id.Should().Be(equipoId);
            resultado.Nombre.Should().Be("Test FC");
            resultado.Puntos.Should().Be(10);
            resultado.DiferenciaGoles.Should().Be(7);
            resultado.GolesFavor.Should().Be(12);
            resultado.GolesContra.Should().Be(5);
            resultado.PartidosJugados.Should().Be(5);
            resultado.PartidosGanados.Should().Be(3);
            resultado.PartidosPerdidos.Should().Be(1);
            resultado.PartidosEmpatados.Should().Be(1);
        }
    }
}