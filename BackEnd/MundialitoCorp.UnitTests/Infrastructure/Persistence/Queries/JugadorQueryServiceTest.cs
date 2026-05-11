using Microsoft.Data.Sqlite;
using Dapper;
using FluentAssertions;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Application.Common;
using MundialitoCorp.Infrastructure.Persistence.Queries;
using MundialitoCorp.UnitTests.Infrastructure.Persistence.Queries;

namespace MundialitoCorp.Tests.Infrastructure.Queries
{
    public class JugadorQueryServiceTests : QueryServiceTestBase
    {
        private readonly JugadorQueryService _queryService;

        public JugadorQueryServiceTests()
        {
            _queryService = new JugadorQueryService(DapperContextMock.Object);
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarJugadorConNombreDeEquipo()
        {
            var equipoId = Guid.NewGuid();
            var jugadorId = Guid.NewGuid();
            await Connection.ExecuteAsync("INSERT INTO Equipos (Id, Nombre) VALUES (@Id, 'Argentina')", new { Id = equipoId });
            await Connection.ExecuteAsync("INSERT INTO Jugadores (Id, Nombre, EquipoId, GolesAnotados) VALUES (@Id, 'Messi', @EId, 10)",
                new { Id = jugadorId, EId = equipoId });

            var resultado = await _queryService.GetByIdAsync(jugadorId);

            resultado.Should().NotBeNull();
            resultado!.Nombre.Should().Be("Messi");
            resultado.EquipoNombre.Should().Be("Argentina");
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarNull_CuandoNoExiste()
        {
            var resultado = await _queryService.GetByIdAsync(Guid.NewGuid());

            resultado.Should().BeNull();
        }

        [Fact]
        public async Task ExistenTodosLosJugadoresAsync_DebeRetornarTrue_CuandoTodosExisten()
        {
            var id1 = Guid.NewGuid();
            var id2 = Guid.NewGuid();

            await Connection.ExecuteAsync(
                "INSERT INTO Jugadores (Id, Nombre, GolesAnotados) VALUES (@Id, @Nombre, 0)",
                new[] {
                    new { Id = id1, Nombre = "J1" },
                    new { Id = id2, Nombre = "J2" }
                });

            var idsParaValidar = new List<Guid> { id1, id2 };

            var resultado = await _queryService.ExistenTodosLosJugadoresAsync(idsParaValidar);

            resultado.Should().BeTrue();
        }

        [Fact]
        public async Task ExistenTodosLosJugadoresAsync_DebeRetornarFalse_CuandoUnoNoExiste()
        {
            var idExistente = Guid.NewGuid();
            await Connection.ExecuteAsync(
                "INSERT INTO Jugadores (Id, Nombre, GolesAnotados) VALUES (@Id, 'J1', 0)",
                new { Id = idExistente.ToString() });

            var resultado = await _queryService.ExistenTodosLosJugadoresAsync(new List<Guid> { idExistente, Guid.NewGuid() });

            resultado.Should().BeFalse();
        }

        [Fact]
        public async Task ExistenTodosLosJugadoresAsync_DebeRetornarTrue_SiListaEstaVacia()
        {
            var resultado = await _queryService.ExistenTodosLosJugadoresAsync(new List<Guid>());

            resultado.Should().BeTrue();
        }
    }
}