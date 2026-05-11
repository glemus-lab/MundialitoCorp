using MundialitoCorp.Application.Features.Equipos.Commands.CreateEquipo;
using FluentValidation.TestHelper;

namespace MundialitoCorp.UnitTests.Application.Features.Equipos
{
    public class CreateEquipoCommandValidatorTests
    {
        private readonly CreateEquipoCommandValidator _validator;

        public CreateEquipoCommandValidatorTests()
        {
            _validator = new CreateEquipoCommandValidator();
        }

        [Fact]
        public void Validator_DebePasar_CuandoElComandoEsValido()
        {
            // Arrange
            var command = new CreateEquipoCommand("Tech FC");

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Validator_DebeFallar_CuandoElNombreEstaVacio(string? nombreInvalido)
        {
            // Arrange
            var command = new CreateEquipoCommand(nombreInvalido!);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Nombre)
                .WithErrorMessage("El nombre del equipo no puede estar vacío.");
        }

        [Fact]
        public void Validator_DebeFallar_CuandoElNombreSuperaLos100Caracteres()
        {
            // Arrange
            var nombreLargo = new string('A', 101);
            var command = new CreateEquipoCommand(nombreLargo);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Nombre)
                .WithErrorMessage("El nombre no puede superar los 100 caracteres.");
        }
    }
}