using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Entities;
using FluentAssertions;
using MundialitoCorp.Domain.Events;

namespace MundialitoCorp.UnitTests.Domain.Entities
{
    public class EquipoTests
    {
        #region Tests de Constructor e Invariantes

        [Fact]
        public void Constructor_DebeInicializarCorrectamente_CuandoDatosSonValidos()
        {
            // Arrange
            var nombre = "Real Madrid";

            // Act
            var equipo = Equipo.Create(nombre).Value!;

            // Assert
            equipo.Id.Should().NotBeEmpty();
            equipo.Nombre.Should().Be(nombre);
            equipo.Puntos.Should().Be(0);
            equipo.PartidosJugados.Should().Be(0);
            equipo.Jugadores.Should().NotBeNull().And.BeEmpty();
            equipo.DomainEvents.Should().ContainSingle(e => e is EquipoCreadoEvent);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null!)]
        public void Constructor_DebeRegresarResultFailure_CuandoNombreEsInvalido(string? nombreInvalido)
        {
            // Act
            var result = Equipo.Create(nombreInvalido!);

            // Assert
            result.Should().BeOfType<Result<Equipo>>();
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be("Error de validación.");
            result.Code.Should().Be(422);
            result.Errors[0].PropertyName.Should().Be("Nombre");
            result.Errors[0].Message.Should().Be("El nombre del equipo no puede estar vacío.");
        }

        [Fact]
        public void Constructor_DebeRegresarResultFailure_CuandoNombreSuperaLos100Caracteres()
        {
            // Act
            var nombre = new string('A', 101);
            var result = Equipo.Create(nombre);

            // Assert
            result.Should().BeOfType<Result<Equipo>>();
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be("Error de validación.");
            result.Code.Should().Be(422);
            result.Errors[0].PropertyName.Should().Be("Nombre");
            result.Errors[0].Message.Should().Be("El nombre no puede superar los 100 caracteres.");
        }

        #endregion

        #region Tests de Lógica de Negocio (Estadísticas)

        [Fact]
        public void ActualizarEstadisticas_DebeAsignar3Puntos_CuandoSeGana()
        {
            var equipo = Equipo.Create("Local").Value!;
            equipo.ActualizarEstadisticas(favor: 2, contra: 0);

            equipo.Puntos.Should().Be(3);
            equipo.PartidosJugados.Should().Be(1);
            equipo.GolesFavor.Should().Be(2);
            equipo.GolesContra.Should().Be(0);
            equipo.DiferenciaGoles.Should().Be(2);
        }

        [Fact]
        public void ActualizarEstadisticas_DebeAsignar1Punto_CuandoSeEmpata()
        {
            var equipo = Equipo.Create("Local").Value!;
            equipo.ActualizarEstadisticas(favor: 1, contra: 1);

            equipo.Puntos.Should().Be(1);
            equipo.DiferenciaGoles.Should().Be(0);
        }

        [Fact]
        public void ActualizarEstadisticas_DebeAsignar0Puntos_CuandoSePierde()
        {
            var equipo = Equipo.Create("Local").Value!;
            equipo.ActualizarEstadisticas(favor: 0, contra: 3);

            equipo.Puntos.Should().Be(0);
            equipo.DiferenciaGoles.Should().Be(-3);
        }

        [Fact]
        public void ActualizarEstadisticas_DebeAcumularDatos_EnMultiplesPartidos()
        {
            var equipo = Equipo.Create("Acumulador").Value!;

            equipo.ActualizarEstadisticas(2, 0);
            equipo.ActualizarEstadisticas(1, 1);

            equipo.Puntos.Should().Be(4);
            equipo.PartidosJugados.Should().Be(2);
            equipo.GolesFavor.Should().Be(3);
            equipo.GolesContra.Should().Be(1);
        }

        [Theory]
        [InlineData(-1, 0)]
        [InlineData(0, -1)]
        public void ActualizarEstadisticas_RegresaFailure_CuandoGolesSonNegativos(int favor, int contra)
        {
            var equipo = Equipo.Create("Acumulador").Value!;

            var result = equipo.ActualizarEstadisticas(favor, contra);

            Assert.IsType<Result>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(422);
            result.ErrorMessage.Should().Be("Los goles no pueden ser negativos.");
        }

        #endregion

        #region Tests de Gestión de Jugadores

        [Fact]
        public void AgregarJugador_DebeAñadirALaColeccion_CuandoHayEspacio()
        {
            var equipo = Equipo.Create("Team").Value!;
            equipo.AgregarJugador("Jugador Uno");

            equipo.Jugadores.Should().HaveCount(1);
            equipo.Jugadores[0].Id.Should().NotBeEmpty();
            equipo.Jugadores[0].Nombre.Should().Be("Jugador Uno");
            equipo.Jugadores[0].EquipoId.Should().Be(equipo.Id);
        }

        [Fact]
        public void AgregarJugador_DebeRegresarResultFailure_CuandoSeAlcanzaLimiteDe25()
        {
            var equipo = Equipo.Create("Full Team").Value!;
            for (int i = 0; i < 25; i++)
                equipo.AgregarJugador($"J{i}");

            var result = equipo.AgregarJugador("Jugador 26");

            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be("El equipo ya tiene el máximo de jugadores.");
        }

        #endregion

        #region Tests de Edición

        [Fact]
        public void CambiarNombre_DebeActualizar_CuandoEsValido()
        {
            var result = Equipo.Create("Nombre Viejo");
            var equipo = result.Value!;

            result.IsSuccess.Should().BeTrue();
            equipo.CambiarNombre("Nombre Nuevo");
            equipo.Nombre.Should().Be("Nombre Nuevo");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null!)]
        public void CambiarNombre_DebeRegresarResultFailure_CuandoNombreEsNuloOVacio(string? nuevoNombre)
        {
            var equipo = Equipo.Create("Test").Value!;

            var result = equipo.CambiarNombre(nuevoNombre!);

            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be("Error de validación.");
            result.Code.Should().Be(422);
            result.Errors[0].PropertyName.Should().Be("Nombre");
            result.Errors[0].Message.Should().Be("El nombre del equipo no puede estar vacío.");
        }

        [Fact]
        public void CambiarNombre_DebeRegresarResultFailure_CuandoNombreSuperaLos100Caracteres()
        {
            var equipo = Equipo.Create("Test").Value!;
            var nuevoNombre = new string('A', 101);

            var result = equipo.CambiarNombre(nuevoNombre!);

            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be("Error de validación.");
            result.Code.Should().Be(422);
            result.Errors[0].PropertyName.Should().Be("Nombre");
            result.Errors[0].Message.Should().Be("El nombre no puede superar los 100 caracteres.");
        }

        #endregion
    }
}