using MundialitoCorp.Application.Features.Jugadores.Commands.CreateJugador;
using FluentValidation.TestHelper;

namespace MundialitoCorp.UnitTests.Application.Features.Jugadores
{
    public class CreateJugadorCommandValidatorTest
    {
        private readonly CreateJugadorCommandValidator _validator;

        public CreateJugadorCommandValidatorTest()
        {
            _validator = new CreateJugadorCommandValidator();
        }

        [Fact]
        public async Task Validator_DebePasar_CuandoElComandoEsValido()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var nombre = "Nombre Jugador";
            var command = new CreateJugadorCommand(nombre, equipoId);

            // Act
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task Validator_DebeFallar_CuandoElNombreNoEsValido(string? nombre)
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var command = new CreateJugadorCommand(nombre!, equipoId);

            // Act
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Nombre)
                .WithErrorMessage("El nombre del jugador es obligatorio.");
        }

        [Fact]
        public async Task Validator_DebeFallar_CuandoElNombreSuperaLos150Caracteres()
        {
            // Arrange
            var equipoId = Guid.NewGuid();
            var nombre = new string('A', 151);
            var command = new CreateJugadorCommand(nombre!, equipoId);

            // Act
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.Nombre)
                .WithErrorMessage("El nombre no puede exceder los 150 caracteres.");
        }

        [Fact]
        public async Task Validator_DebeFallar_CuandoElEquipoIdEsVacio()
        {
            // Arrange
            var nombre = new string('A', 151);
            var equipoId = Guid.Empty;
            var command = new CreateJugadorCommand(nombre!, equipoId);

            // Act
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.EquipoId)
                .WithErrorMessage("El ID del equipo es obligatorio.");
        }
    }
}
