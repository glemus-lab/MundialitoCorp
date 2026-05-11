using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Infrastructure.Persistence;
using MundialitoCorp.Infrastructure.Persistence.Repositories;
using FluentAssertions;
using Xunit;

namespace MundialitoCorp.Tests.Infrastructure
{
    public class JugadorRepositoryTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public JugadorRepositoryTests()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

            using var context = new ApplicationDbContext(_options);
            context.Database.EnsureCreated();
        }

        private ApplicationDbContext CreateContext() => new ApplicationDbContext(_options);

        [Fact]
        public async Task AddAsync_DebeGuardarJugadorCorrectamente()
        {
            // Arrange
            Guid jugadorId;
            Guid equipoId;

            using (var contextArrange = CreateContext())
            {
                var equipo = Equipo.Create("Equipo Test").Value!;
                contextArrange.Equipos.Add(equipo);
                await contextArrange.SaveChangesAsync();
                equipoId = equipo.Id;

                var jugador = Jugador.Create("Jugador Nuevo", equipoId).Value!;
                jugadorId = jugador.Id;

                // Act
                var repository = new JugadorRepository(contextArrange);
                await repository.AddAsync(jugador);
                await contextArrange.SaveChangesAsync();
            }

            // Assert
            using (var contextAssert = CreateContext())
            {
                var jugadorEnBaseDeDatos = await contextAssert.Jugadores.FindAsync(jugadorId);
                jugadorEnBaseDeDatos.Should().NotBeNull();
                jugadorEnBaseDeDatos!.Nombre.Should().Be("Jugador Nuevo");
                jugadorEnBaseDeDatos.EquipoId.Should().Be(equipoId);
            }
        }

        [Fact]
        public async Task GetByIdAsync_DebeRetornarJugador_SiExiste()
        {
            // Arrange
            Guid jugadorId;
            using (var contextArrange = CreateContext())
            {
                var equipo = Equipo.Create("Equipo Test").Value!;
                contextArrange.Equipos.Add(equipo);
                await contextArrange.SaveChangesAsync();

                var jugador = Jugador.Create("Jugador Existente", equipo.Id).Value!;
                contextArrange.Jugadores.Add(jugador);
                await contextArrange.SaveChangesAsync();
                jugadorId = jugador.Id;
            }

            // Act
            using (var contextAct = CreateContext())
            {
                var repository = new JugadorRepository(contextAct);
                var resultado = await repository.GetByIdAsync(jugadorId);

                // Assert
                resultado.Should().NotBeNull();
                resultado!.Id.Should().Be(jugadorId);
                resultado.Nombre.Should().Be("Jugador Existente");
            }
        }

        [Fact]
        public async Task Update_DebeModificarNombreDelJugador()
        {
            // Arrange
            Guid jugadorId;
            using (var contextArrange = CreateContext())
            {
                var equipo = Equipo.Create("Equipo Test").Value!;
                contextArrange.Equipos.Add(equipo);
                await contextArrange.SaveChangesAsync();

                var jugador = Jugador.Create("Nombre Original", equipo.Id).Value!;
                contextArrange.Jugadores.Add(jugador);
                await contextArrange.SaveChangesAsync();
                jugadorId = jugador.Id;
            }

            // Act
            using (var contextAct = CreateContext())
            {
                var repository = new JugadorRepository(contextAct);
                var jugadorParaActualizar = await repository.GetByIdAsync(jugadorId);

                jugadorParaActualizar!.CambiarNombre("Nombre Actualizado");

                repository.Update(jugadorParaActualizar);
                await contextAct.SaveChangesAsync();
            }

            // Assert
            using (var contextAssert = CreateContext())
            {
                var jugadorEnBaseDeDatos = await contextAssert.Jugadores.FindAsync(jugadorId);
                jugadorEnBaseDeDatos!.Nombre.Should().Be("Nombre Actualizado");
            }
        }

        [Fact]
        public async Task DeleteAsync_DebeEliminarJugador_SiExiste()
        {
            // Arrange
            Guid jugadorId;
            using (var contextArrange = CreateContext())
            {
                var equipo = Equipo.Create("Equipo Test").Value!;
                contextArrange.Equipos.Add(equipo);
                await contextArrange.SaveChangesAsync();

                var jugador = Jugador.Create("Jugador a Eliminar", equipo.Id).Value!;
                contextArrange.Jugadores.Add(jugador);
                await contextArrange.SaveChangesAsync();
                jugadorId = jugador.Id;
            }

            // Act
            using (var contextAct = CreateContext())
            {
                var repository = new JugadorRepository(contextAct);
                await repository.DeleteAsync(jugadorId);
                await contextAct.SaveChangesAsync();
            }

            // Assert
            using (var contextAssert = CreateContext())
            {
                var existeJugador = await contextAssert.Jugadores.AnyAsync(j => j.Id == jugadorId);
                existeJugador.Should().BeFalse();
            }
        }

        [Fact]
        public async Task GetByEquipoIdAsync_DebeRetornarTodosLosJugadoresDelEquipo()
        {
            // Arrange
            Guid equipoId;
            using (var contextArrange = CreateContext())
            {
                var equipo = Equipo.Create("Equipo Real").Value!;
                var equipoOtro = Equipo.Create("Otro Equipo").Value!;
                contextArrange.Equipos.AddRange(equipo, equipoOtro);
                await contextArrange.SaveChangesAsync();
                equipoId = equipo.Id;

                contextArrange.Jugadores.AddRange(
                    Jugador.Create("Jugador 1", equipo.Id).Value!,
                    Jugador.Create("Jugador 2", equipo.Id).Value!,
                    Jugador.Create("Jugador 3", equipoOtro.Id).Value!
                );
                await contextArrange.SaveChangesAsync();
            }

            // Act
            using (var contextAct = CreateContext())
            {
                var repository = new JugadorRepository(contextAct);
                var jugadores = await repository.GetByEquipoIdAsync(equipoId);

                // Assert
                jugadores.Should().HaveCount(2);
                jugadores.Should().OnlyContain(j => j.EquipoId == equipoId);
            }
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}