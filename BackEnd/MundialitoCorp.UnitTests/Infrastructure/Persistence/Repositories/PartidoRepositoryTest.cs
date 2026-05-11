using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.ValueObjects;
using MundialitoCorp.Infrastructure.Persistence;
using MundialitoCorp.Infrastructure.Persistence.Repositories;
using FluentAssertions;

namespace MundialitoCorp.Tests.Infrastructure
{
    public class PartidoRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public PartidoRepositoryTests()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .EnableSensitiveDataLogging()
                .Options;

            using (var context = new ApplicationDbContext(_options))
            {
                context.Database.EnsureCreated();
            }
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options);

        [Fact]
        public async Task AddAsync_DebeGuardarPartidoCorrectamente()
        {
            // Arrange
            Guid partidoId;
            Guid equipoLocalId;

            using (var contextArrange = CreateContext())
            {
                var equipoLocal = Equipo.Create("Local FC").Value!;
                var equipoVisitante = Equipo.Create("Visitante FC").Value!;
                contextArrange.Equipos.AddRange(equipoLocal, equipoVisitante);
                await contextArrange.SaveChangesAsync();

                equipoLocalId = equipoLocal.Id;
                var partido = Partido.Create(equipoLocal.Id, equipoVisitante.Id, DateOnly.FromDateTime(DateTime.Now)).Value!;
                partidoId = partido.Id;

                // Act
                var repository = new PartidoRepository(contextArrange);
                await repository.AddAsync(partido);
                await contextArrange.SaveChangesAsync();
            }

            // Assert
            using (var contextAssert = CreateContext())
            {
                var partidoEnBaseDeDatos = await contextAssert.Partidos.FindAsync(partidoId);
                partidoEnBaseDeDatos.Should().NotBeNull();
                partidoEnBaseDeDatos!.EquipoLocalId.Should().Be(equipoLocalId);
            }
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarPartido_SiExiste()
        {
            // Arrange
            Guid partidoId;
            using (var contextArrange = CreateContext())
            {
                var equipoLocal = Equipo.Create("L").Value!;
                var equipoVisitante = Equipo.Create("V").Value!;
                contextArrange.Equipos.AddRange(equipoLocal, equipoVisitante);
                var partido = Partido.Create(equipoLocal.Id, equipoVisitante.Id, DateOnly.FromDateTime(DateTime.Now)).Value!;
                contextArrange.Partidos.Add(partido);
                await contextArrange.SaveChangesAsync();
                partidoId = partido.Id;
            }

            // Act
            using (var contextAct = CreateContext())
            {
                var repository = new PartidoRepository(contextAct);
                var result = await repository.GetByIdAsync(partidoId);

                // Assert
                result.Should().NotBeNull();
                result!.Id.Should().Be(partidoId);
            }
        }

        [Fact]
        public async Task Update_DebePersistirGoleadores_Y_FinalizarPartido()
        {
            // Arrange
            Guid partidoId;
            Guid jugadorLocalId;
            Guid jugadorVisitanteId;

            using (var contextArrange = CreateContext())
            {
                var equipoLocal = Equipo.Create("Equipo Local").Value!;
                var equipoVisitante = Equipo.Create("Equipo Visitante").Value!;
                contextArrange.Equipos.AddRange(equipoLocal, equipoVisitante);
                await contextArrange.SaveChangesAsync();

                var jugadorLocal = Jugador.Create("Goleador Local", equipoLocal.Id).Value!;
                var jugadorVisitante = Jugador.Create("Goleador Visitante", equipoVisitante.Id).Value!;
                contextArrange.Jugadores.AddRange(jugadorLocal, jugadorVisitante);

                var partido = Partido.Create(equipoLocal.Id, equipoVisitante.Id, DateOnly.FromDateTime(DateTime.Now)).Value!;
                contextArrange.Partidos.Add(partido);
                await contextArrange.SaveChangesAsync();

                partidoId = partido.Id;
                jugadorLocalId = jugadorLocal.Id;
                jugadorVisitanteId = jugadorVisitante.Id;
            }

            // Act
            using (var contextAct = CreateContext())
            {
                var partidoParaActualizar = await contextAct.Partidos
                    .Include(p => p.GolesDetalle)
                    .FirstOrDefaultAsync(p => p.Id == partidoId);

                var resultado = Resultado.Create(1, 1).Value!;
                var goleadoresLocales = new List<Guid> { jugadorLocalId };
                var goleadoresVisitantes = new List<Guid> { jugadorVisitanteId };

                partidoParaActualizar!.RegistrarResultado(resultado, goleadoresLocales, goleadoresVisitantes);

                contextAct.Entry(partidoParaActualizar).State = EntityState.Modified;
                foreach (var gol in partidoParaActualizar.GolesDetalle)
                {
                    contextAct.Entry(gol).State = EntityState.Added;
                }

                await contextAct.SaveChangesAsync();
            }

            // Assert
            using (var contextAssert = CreateContext())
            {
                var partidoEnBaseDeDatos = await contextAssert.Partidos
                    .Include(p => p.GolesDetalle)
                    .FirstOrDefaultAsync(p => p.Id == partidoId);

                partidoEnBaseDeDatos.Should().NotBeNull();
                partidoEnBaseDeDatos!.EstaFinalizado.Should().BeTrue();
                partidoEnBaseDeDatos.GolesDetalle.Should().HaveCount(2);
            }
        }

        [Fact]
        public async Task DeleteAsync_DebeEliminarPartidoYSusGolesEnCascada()
        {
            // Arrange
            Guid partidoId;
            using (var contextArrange = CreateContext())
            {
                var equipoLocal = Equipo.Create("L").Value!;
                var equipoVisitante = Equipo.Create("V").Value!;
                var jugador = Jugador.Create("G", equipoLocal.Id).Value!;
                contextArrange.Equipos.AddRange(equipoLocal, equipoVisitante);
                contextArrange.Jugadores.Add(jugador);
                await contextArrange.SaveChangesAsync();

                var partido = Partido.Create(equipoLocal.Id, equipoVisitante.Id, DateOnly.FromDateTime(DateTime.Now)).Value!;
                partido.RegistrarResultado(Resultado.Create(1, 0).Value!, new List<Guid> { jugador.Id }, new List<Guid>());
                contextArrange.Partidos.Add(partido);
                await contextArrange.SaveChangesAsync();
                partidoId = partido.Id;
            }

            // Act
            using (var contextAct = CreateContext())
            {
                var repository = new PartidoRepository(contextAct);
                await repository.DeleteAsync(partidoId);
                await contextAct.SaveChangesAsync();
            }

            // Assert
            using (var contextAssert = CreateContext())
            {
                var existePartido = await contextAssert.Partidos.AnyAsync(p => p.Id == partidoId);
                var existenGoles = await contextAssert.Set<PartidoGol>().AnyAsync(g => g.PartidoId == partidoId);

                existePartido.Should().BeFalse();
                existenGoles.Should().BeFalse();
            }
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}