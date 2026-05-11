using MundialitoCorp.Application.Features.Partidos.Commands.UpdateFechaPartido;
using FluentValidation.TestHelper;

namespace MundialitoCorp.UnitTests.Application.Features.Partidos
{
    public class UpdateFechaPartidoCommandValidatorTest
    {
        private readonly UpdateFechaPartidoCommandValidator _validator;

        public UpdateFechaPartidoCommandValidatorTest()
        {
            _validator = new();
        }

        [Fact]
        public async Task Validator_DebePasar_CuandoCommandEsValido()
        {
            // Arrange
            var command = new UpdateFechaPartidoCommand(Guid.NewGuid(), DateOnly.FromDateTime(DateTime.Now.AddDays(1)));

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}