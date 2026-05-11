using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Events;
using MundialitoCorp.Domain.ValueObjects;
using FluentAssertions;

namespace MundialitoCorp.UnitTests.Domain.Entities
{
    public class PartidoTests
    {
        #region Tests de Constructor e Inicialización

        [Fact]
        public void Constructor_DebeInicializarCorrectamente_CuandoDatosSonValidos_ConFechaDeAhora()
        {
            // Arrange
            var localId = Guid.NewGuid();
            var visitanteId = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);

            // Act
            var result = Partido.Create(localId, visitanteId, fecha);
            var partido = result.Value!;

            // Assert
            result.IsSuccess.Should().BeTrue();
            partido.Id.Should().NotBeEmpty();
            partido.EquipoLocalId.Should().Be(localId);
            partido.EquipoVisitanteId.Should().Be(visitanteId);
            partido.Fecha.Should().Be(fecha);
            partido.EstaFinalizado.Should().BeFalse();
            partido.GolesLocal.Should().BeNull();
            partido.GolesVisitante.Should().BeNull();
        }

        [Fact]
        public void Constructor_DebeInicializarCorrectamente_CuandoDatosSonValidos_ConFechaAFuturo()
        {
            // Arrange
            var localId = Guid.NewGuid();
            var visitanteId = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(1));

            // Act
            var result = Partido.Create(localId, visitanteId, fecha);
            var partido = result.Value!;

            // Assert
            result.IsSuccess.Should().BeTrue();
            partido.Id.Should().NotBeEmpty();
            partido.EquipoLocalId.Should().Be(localId);
            partido.EquipoVisitanteId.Should().Be(visitanteId);
            partido.Fecha.Should().Be(fecha);
            partido.EstaFinalizado.Should().BeFalse();
            partido.GolesLocal.Should().BeNull();
            partido.GolesVisitante.Should().BeNull();
        }

        [Fact]
        public void Constructor_RegresaResultFailure_CuandoLosIdsDeEquiposEsElMismo()
        {
            // Arrange
            var localId = Guid.NewGuid();
            var visitanteId = localId;
            var fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(1));

            // Act
            var result = Partido.Create(localId, visitanteId, fecha);
            var partido = result.Value!;

            // Assert
            Assert.IsType<Result<Partido>>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(422);
            result.ErrorMessage.Should().Be("Los equipos rivales no puede ser el mismo.");
        }

        [Fact]
        public void Constructor_RegresaResultFailure_CuandoLaFechaEsDelPasado()
        {
            // Arrange
            var localId = Guid.NewGuid();
            var visitanteId = localId;
            var fecha = DateOnly.FromDateTime(DateTime.Now.AddDays(-1));

            // Act
            var result = Partido.Create(localId, visitanteId, fecha);
            var partido = result.Value!;

            // Assert
            Assert.IsType<Result<Partido>>(result);
            result.IsFailure.Should().BeTrue();
            result.Code.Should().Be(422);
            result.ErrorMessage.Should().Be("Los equipos rivales no puede ser el mismo.");
        }

        #endregion

        #region Tests de Registro de Resultados (Lógica de Negocio)

        [Fact]
        public void RegistrarResultado_DebeFinalizarPartidoYAsignarGoles_CuandoEsExitoso()
        {
            // Arrange
            var partido = Partido.Create(Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now)).Value!;
            var goleadoresL = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() };
            var goleadoresV = new List<Guid> { Guid.NewGuid() };
            var resultado = Resultado.Create(2, 1).Value!;

            // Act
            var result = partido.RegistrarResultado(resultado, goleadoresL, goleadoresV);

            // Assert
            result.IsSuccess.Should().BeTrue();
            partido.EstaFinalizado.Should().BeTrue();
            partido.GolesLocal.Should().Be(2);
            partido.GolesVisitante.Should().Be(1);
        }

        [Fact]
        public void RegistrarResultado_DebeDispararEventoDeDominio_CuandoSeFinaliza()
        {
            // Arrange
            var partido = Partido.Create(Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now)).Value!;
            var resultado = Resultado.Create(1, 0).Value!;

            // Act
            partido.RegistrarResultado(resultado, [Guid.NewGuid()], []);

            // Assert
            var domainEvent = partido.DomainEvents.FirstOrDefault();
            domainEvent.Should().NotBeNull();
            domainEvent.Should().BeOfType<ResultadoRegistradoEvent>();
            var @event = (ResultadoRegistradoEvent)domainEvent!;
            @event.GolesL.Should().Be(1);
            @event.GolesV.Should().Be(0);
        }

        [Fact]
        public void RegistrarResultado_DebeRetornarFalla_CuandoElPartidoYaFueFinalizado()
        {
            // Arrange
            var partido = Partido.Create(Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now)).Value!;
            var resutlado1 = Resultado.Create(0, 0).Value!;
            var resultado2 = Resultado.Create(1, 1).Value!;
            partido.RegistrarResultado(resutlado1, [], []);

            // Act
            var result = partido.RegistrarResultado(resultado2, [Guid.NewGuid()], [Guid.NewGuid()]);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be("El partido ya ha sido finalizado previamente.");
            result.Code.Should().Be(422);
        }

        [Fact]
        public void RegistrarResultado_DebeRetornarFalla_CuandoLaFechaDePartidoEsAFuturo()
        {
            // Arrange
            var partido = Partido.Create(Guid.NewGuid(), Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now.AddDays(2))).Value!;
            var resutlado1 = Resultado.Create(0, 0).Value!;
            var resultado2 = Resultado.Create(1, 1).Value!;
            partido.RegistrarResultado(resutlado1, new(), new());

            // Act
            var result = partido.RegistrarResultado(resultado2, new(), new());

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be("No se pueden registrar resultado de un partido que no se ha disputado.");
            result.Code.Should().Be(422);
        }

        #endregion
    }
}