using MundialitoCorp.Application.Features.Equipos.Commands.UpdateEquipo;
using FluentValidation.TestHelper;

namespace MundialitoCorp.UnitTests.Application.Features.Equipos
{
    public class UpdateEquipoCommandValidatorTests
    {
        private readonly UpdateEquipoCommandValidator _validator;

        public UpdateEquipoCommandValidatorTests()
        {
            _validator = new UpdateEquipoCommandValidator();
        }

        [Fact]
        public void Validator_DebePasar_CuandoElComandoEsValido()
        {
            // Arrange
            var command = new UpdateEquipoCommand(Guid.NewGuid(), "Nombre Valido");

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validator_DebeFallar_CuandoElIdEstaVacio()
        {
            // Arrange
            var command = new UpdateEquipoCommand(Guid.Empty, "Nombre Valido");

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage("El ID del equipo es obligatorio.");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void Validator_DebeFallar_CuandoElNombreEsInvalido(string? nombreInvalido)
        {
            // Arrange
            var command = new UpdateEquipoCommand(Guid.NewGuid(), nombreInvalido!);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Nombre)
                .WithErrorMessage("El nombre del equipo es obligatorio.");
        }

        [Fact]
        public void Validator_DebeFallar_CuandoElNombreSuperaLos100Caracteres()
        {
            // Arrange
            var nombreLargo = new string('A', 101);
            var command = new UpdateEquipoCommand(Guid.NewGuid(), nombreLargo);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Nombre)
                .WithErrorMessage("El nombre no puede superar los 100 caracteres.");
        }
    }
}