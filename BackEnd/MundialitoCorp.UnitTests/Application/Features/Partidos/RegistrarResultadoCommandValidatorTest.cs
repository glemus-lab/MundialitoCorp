using MundialitoCorp.Application.Features.Partidos.Commands.RegistrarResultado;
using FluentValidation.TestHelper;

namespace MundialitoCorp.UnitTests.Application.Features.Partidos
{
    public class RegistrarResultadoCommandValidatorTest
    {
        private readonly RegistrarResultadoCommandValidator _validator;

        public RegistrarResultadoCommandValidatorTest()
        {
            _validator = new();
        }

        [Fact]
        public async Task Validator_DebePasar_CuandoCommandEsValido()
        {
            // Arrange
            var partidoId = Guid.NewGuid();
            var golesL = 1;
            var golesV = 1;
            var goleadoresL = new List<Guid>() { Guid.NewGuid() };
            var goleadoresV = new List<Guid> { Guid.NewGuid() };

            var command = new RegistrarResultadoCommand(partidoId, golesL, golesV, goleadoresL, goleadoresV);

            // Act
            var result = _validator.TestValidate(command);

            // Arrange
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validator_DebeFallar_CuandoPartidoIdEsEmpty()
        {
            // Arrange
            var partidoId = Guid.Empty;
            var golesL = 1;
            var golesV = 1;
            var goleadoresL = new List<Guid>() { Guid.NewGuid() };
            var goleadoresV = new List<Guid> { Guid.NewGuid() };

            var command = new RegistrarResultadoCommand(partidoId, golesL, golesV, goleadoresL, goleadoresV);

            // Act
            var result = _validator.TestValidate(command);

            // Arrange
            result.ShouldHaveValidationErrorFor(s => s.PartidoId)
                .WithErrorMessage("El ID del partido es obligatorio.");
        }

        [Fact]
        public async Task Validator_DebeFallar_CuandoGolesLocalEsNegativo()
        {
            // Arrange
            var partidoId = Guid.NewGuid();
            var golesL = -1;
            var golesV = 1;
            var goleadoresL = new List<Guid>() { Guid.NewGuid() };
            var goleadoresV = new List<Guid> { Guid.NewGuid() };

            var command = new RegistrarResultadoCommand(partidoId, golesL, golesV, goleadoresL, goleadoresV);

            // Act
            var result = _validator.TestValidate(command);

            // Arrange
            result.ShouldHaveValidationErrorFor(s => s.GolesLocal)
                .WithErrorMessage("Los goles no pueden ser negativos.");
        }

        [Fact]
        public async Task Validator_DebeFallar_CuandoGolesVisitanteEsNegativo()
        {
            // Arrange
            var partidoId = Guid.NewGuid();
            var golesL = 1;
            var golesV = -1;
            var goleadoresL = new List<Guid>() { Guid.NewGuid() };
            var goleadoresV = new List<Guid> { Guid.NewGuid() };

            var command = new RegistrarResultadoCommand(partidoId, golesL, golesV, goleadoresL, goleadoresV);

            // Act
            var result = _validator.TestValidate(command);

            // Arrange
            result.ShouldHaveValidationErrorFor(s => s.GolesVisitante)
                .WithErrorMessage("Los goles no pueden ser negativos.");
        }

        [Fact]
        public async Task Validator_DebeFallar_CuandoGoleadoresLocalIdsNoCoincideConGolesLocal()
        {
            // Arrange
            var partidoId = Guid.NewGuid();
            var golesL = 2;
            var golesV = 1;
            var goleadoresL = new List<Guid>() { Guid.NewGuid() };
            var goleadoresV = new List<Guid> { Guid.NewGuid() };

            var command = new RegistrarResultadoCommand(partidoId, golesL, golesV, goleadoresL, goleadoresV);

            // Act
            var result = _validator.TestValidate(command);

            // Arrange
            result.ShouldHaveValidationErrorFor(s => s.GoleadoresLocalIds)
                .WithErrorMessage("La lista de goleadores locales debe coincidir con el número de goles anotados.");
        }

        [Fact]
        public async Task Validator_DebeFallar_CuandoGoleadoresVisitanteIdsNoCoincideConGolesVisitante()
        {
            // Arrange
            var partidoId = Guid.NewGuid();
            var golesL = 1;
            var golesV = 2;
            var goleadoresL = new List<Guid>() { Guid.NewGuid() };
            var goleadoresV = new List<Guid> { Guid.NewGuid() };

            var command = new RegistrarResultadoCommand(partidoId, golesL, golesV, goleadoresL, goleadoresV);

            // Act
            var result = _validator.TestValidate(command);

            // Arrange
            result.ShouldHaveValidationErrorFor(s => s.GoleadoresVisitanteIds)
                .WithErrorMessage("La lista de goleadores visitantes debe coincidir con el número de goles anotados.");
        }
    }
}