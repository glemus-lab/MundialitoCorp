using MundialitoCorp.Application.Features.Partidos.Commands.CreatePartido;
using FluentValidation.TestHelper;

namespace MundialitoCorp.UnitTests.Application.Features.Partidos
{
    public class CreatePartidoCommandValidatorTest
    {
        private readonly CreatePartidoCommandValidator _validator;

        public CreatePartidoCommandValidatorTest()
        {
            _validator = new CreatePartidoCommandValidator();
        }

        [Fact]
        public async Task Validator_DebePasar_CuandoCommandEsValido()
        {
            // Arrange
            var localId = Guid.NewGuid();
            var visitanteId = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);

            var command = new CreatePartidoCommand(localId, visitanteId, fecha);

            // Act
            var result = _validator.TestValidate(command);


            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [MemberData(nameof(GetIdsInvalidos))]
        public async Task Validator_DebeFallar_CuandoNoSeEnviaEquipoLocalId(Guid? guid)
        {
            // Arrange
            Guid? localId = guid;
            var visitanteId = Guid.NewGuid();
            var fecha = DateOnly.FromDateTime(DateTime.Now);

            var command = new CreatePartidoCommand(localId, visitanteId, fecha);

            // Act
            var result = _validator.TestValidate(command);


            // Assert
            result.ShouldHaveValidationErrorFor(s => s.EquipoLocalId)
                .WithErrorMessage("El equipo local es obligatorio.");
        }

        [Theory]
        [MemberData(nameof(GetIdsInvalidos))]
        public async Task Validator_DebeFallar_CuandoNoSeEnviaEquipoVisitanteId(Guid? guid)
        {
            // Arrange
            Guid? localId = Guid.NewGuid();
            var visitanteId = guid;
            var fecha = DateOnly.FromDateTime(DateTime.Now);

            var command = new CreatePartidoCommand(localId, visitanteId, fecha);

            // Act
            var result = _validator.TestValidate(command);


            // Assert
            result.ShouldHaveValidationErrorFor(s => s.EquipoVisitanteId)
                .WithErrorMessage("El equipo visitante es obligatorio.");
        }

        [Fact]
        public async Task Validator_DebeFallar_CuandoEquipoLocalIdYEquipoVisitanteIdSonIguales()
        {
            // Arrange
            Guid? localId = Guid.NewGuid();
            var visitanteId = localId;
            var fecha = DateOnly.FromDateTime(DateTime.Now);

            var command = new CreatePartidoCommand(localId, visitanteId, fecha);

            // Act
            var result = _validator.TestValidate(command);


            // Assert
            result.ShouldHaveValidationErrorFor(s => s.EquipoVisitanteId)
                .WithErrorMessage("Un equipo no puede jugar contra sí mismo.");
        }

        [Fact]
        public async Task Validator_DebeFallar_CuandoNoSeEnviaLaFecha()
        {
            // Arrange
            Guid? localId = Guid.NewGuid();
            var visitanteId = localId;
            DateOnly? fecha = null;

            var command = new CreatePartidoCommand(localId, visitanteId, fecha);

            // Act
            var result = _validator.TestValidate(command);


            // Assert
            result.ShouldHaveValidationErrorFor(s => s.Fecha)
                .WithErrorMessage("La fecha del partido es obligatoria.");
        }

        public static IEnumerable<object[]> GetIdsInvalidos()
        {
            yield return new object[] { Guid.Empty };
            yield return new object[] { null! };
        }
    }
}
