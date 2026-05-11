using MundialitoCorp.Application.Features.Jugadores.Commands.UpdateJugador;
using FluentValidation.TestHelper;

namespace MundialitoCorp.UnitTests.Application.Features.Jugadores
{
    public class UpdateJugadorCommandValidatorTest
    {
        private readonly UpdateJugadorCommandValidator _validator;

        public UpdateJugadorCommandValidatorTest()
        {
            _validator = new UpdateJugadorCommandValidator();
        }

        [Fact]
        public async Task Handle_DebePasar_CuandoCommandEsValido()
        {
            // Arrange
            var command = new UpdateJugadorCommand(Guid.NewGuid(), "Nombre Jugador");

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Handle_DebeFallar_CuandoIdNoEsValido()
        {
            // Arrange
            var command = new UpdateJugadorCommand(Guid.Empty, "Nombre Jugador");

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(s => s.Id)
                .WithErrorMessage("El ID del jugador es obligatorio.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Handle_DebeFallar_CuandoNombreEsVacio(string? nombre)
        {
            // Arrange
            var command = new UpdateJugadorCommand(Guid.Empty, nombre!);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(s => s.Nombre)
                .WithErrorMessage("El nombre del jugador es obligatorio.");
        }

        [Fact]
        public async Task Handle_DebeFallar_CuandoNombreSuperaLos150Caracteres()
        {
            // Arrange
            var nombre = new string('A', 151);
            var command = new UpdateJugadorCommand(Guid.Empty, nombre);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(s => s.Nombre)
                .WithErrorMessage("El nombre no puede exceder los 150 caracteres.");
        }
    }
}
