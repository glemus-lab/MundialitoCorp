using MundialitoCorp.Domain.Entities;
using FluentAssertions;

namespace MundialitoCorp.UnitTests.Domain.Entities
{
    public class JugadorTests
    {
        #region Tests de Constructor e Invariantes

        [Fact]
        public void Constructor_DebeInicializarCorrectamente_CuandoDatosSonValidos()
        {
            // Arrange
            var nombre = "Lionel Messi";
            var equipoId = Guid.NewGuid();

            // Act
            var result = Jugador.Create(nombre, equipoId);
            var jugador = result.Value!;

            // Assert
            result.IsSuccess.Should().BeTrue();
            jugador.Id.Should().NotBeEmpty();
            jugador.Nombre.Should().Be(nombre);
            jugador.EquipoId.Should().Be(equipoId);
            jugador.GolesAnotados.Should().Be(0);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null!)]
        public void Constructor_DebeRegresarFailure_CuandoNombreEsInvalido(string? nombreInvalido)
        {
            // Arrange
            var equipoId = Guid.NewGuid();

            // Act
            var result = Jugador.Create(nombreInvalido!, equipoId);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be("Error de validación.");
            result.Errors[0].PropertyName.Should().Be("Nombre");
            result.Errors[0].Message.Should().Be("El nombre del jugador no puede estar vacío.");
            result.Code.Should().Be(422);
        }

        [Fact]
        public void Constructor_DebeRegresarFailure_CuandoNombreSuperaLos150Caracteres()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var nombre = new string('A', 151);

            // Act
            var result = Jugador.Create(nombre, equipoId);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be("Error de validación.");
            result.Errors[0].PropertyName.Should().Be("Nombre");
            result.Errors[0].Message.Should().Be("El nombre no puede contener más de 150 caracteres.");
            result.Code.Should().Be(422);
        }

        [Fact]
        public void Constructor_DebeResultFailure_CuandoEquipoIdEsInvalido()
        {
            // Arrange
            var equipoId = Guid.Empty;

            // Act
            var result = Jugador.Create("Nombre de equipo", equipoId);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be("El Id del equipo es requerido.");
            result.Code.Should().Be(422);
        }

        #endregion

        #region Tests de Lógica de Negocio (Goles)

        [Fact]
        public void RegistrarGol_DebeIncrementarContador_EnCadaLlamada()
        {
            // Arrange
            var jugador = Jugador.Create("Cristiano Ronaldo", Guid.NewGuid()).Value!;

            // Act
            jugador.RegistrarGol();
            jugador.RegistrarGol();
            jugador.RegistrarGol();

            // Assert
            jugador.GolesAnotados.Should().Be(3);
        }

        #endregion

        #region Tests de Edición y Comportamiento

        [Fact]
        public void CambiarNombre_DebeActualizar_CuandoEsValido()
        {
            // Arrange
            var jugador = Jugador.Create("Nombre Antiguo", Guid.NewGuid()).Value!;
            var nuevoNombre = "Nombre Nuevo";

            // Act
            var result = jugador.CambiarNombre(nuevoNombre);

            // Assert
            result.IsSuccess.Should().BeTrue();
            jugador.Nombre.Should().Be(nuevoNombre);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null!)]
        public void CambiarNombre_DebeRegresarResultFailure_CuandoNombreEsInvalido(string? nombreInvalido)
        {
            // Arrange
            var jugador = Jugador.Create("Jugador Test", Guid.NewGuid()).Value!;

            // Act
            var result = jugador.CambiarNombre(nombreInvalido!);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be("Error al actualizar el nombre del jugador.");
            result.Errors[0].PropertyName.Should().Be("Nombre");
            result.Errors[0].Message.Should().Be("El nombre del jugador no puede estar vacío.");
            result.Code.Should().Be(422);
        }

        [Fact]
        public void CambiarNombre_DebeRegresarResultFailure_CuandoNombreSuperaLos150Caracteres()
        {
            // Arrange
            var jugador = Jugador.Create("Jugador Test", Guid.NewGuid()).Value!;
            var nuevoNombre = new string('A', 151);

            // Act
            var result = jugador.CambiarNombre(nuevoNombre);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.ErrorMessage.Should().Be("Error al actualizar el nombre del jugador.");
            result.Errors[0].PropertyName.Should().Be("Nombre");
            result.Errors[0].Message.Should().Be("El nombre no puede contener más de 150 caracteres.");
            result.Code.Should().Be(422);
        }

        #endregion
    }
}