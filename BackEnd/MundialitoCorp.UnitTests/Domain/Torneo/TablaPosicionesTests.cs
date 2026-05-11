using MundialitoCorp.Domain.Entities;
using FluentAssertions;

namespace MundialitoCorp.UnitTests.Domain.Torneo
{
    public class TablaPosicionesTests
    {
        [Fact]
        public void Tabla_DebePriorizarDiferenciaGoles_CuandoPuntosSonIguales()
        {
            // Arrange
            var equipoA = Equipo.Create("Equipo A").Value!;
            var equipoB = Equipo.Create("Equipo B").Value!;

            equipoA.ActualizarEstadisticas(2, 1);
            equipoB.ActualizarEstadisticas(3, 0);

            var lista = new List<Equipo> { equipoA, equipoB };

            // Act
            var resultado = lista
                .OrderByDescending(e => e.Puntos)
                .ThenByDescending(e => e.DiferenciaGoles)
                .ToList();

            // Assert
            resultado[0].Nombre.Should().Be("Equipo B");
        }

        [Fact]
        public void Tabla_DebePriorizarGolesFavor_CuandoPuntosYDG_SonIguales()
        {
            // Arrange
            var equipoA = Equipo.Create("Equipo A").Value!;
            var equipoB = Equipo.Create("Equipo B").Value!;

            equipoA.ActualizarEstadisticas(3, 1);
            equipoB.ActualizarEstadisticas(4, 2);

            var lista = new List<Equipo> { equipoA, equipoB };

            // Act
            var resultado = lista
                .OrderByDescending(e => e.Puntos)
                .ThenByDescending(e => e.DiferenciaGoles)
                .ThenByDescending(e => e.GolesFavor)
                .ToList();

            // Assert
            resultado[0].Nombre.Should().Be("Equipo B");
        }
    }
}