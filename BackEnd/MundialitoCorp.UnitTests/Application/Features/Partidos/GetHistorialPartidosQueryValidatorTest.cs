using MundialitoCorp.Application.Features.Partidos.Queries.GetHistorialPartidos;
using FluentValidation.TestHelper;

namespace MundialitoCorp.UnitTests.Application.Features.Partidos
{
    public class GetHistorialPartidosQueryValidatorTest
    {
        private readonly GetHistorialPartidosQueryValidator _validator;

        public GetHistorialPartidosQueryValidatorTest()
        {
            _validator = new GetHistorialPartidosQueryValidator();
        }

        [Fact]
        public async Task Validator_DebePasar_CuandoQueryEsValido()
        {
            // Arramge
            var query = new GetHistorialPartidosQuery(1, 5);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validator_DebeFallar_CuandoPageNumberNoEsValido(int pageNumber)
        {
            // Arramge
            var query = new GetHistorialPartidosQuery(pageNumber, 5);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(s => s.PageNumber)
                .WithErrorMessage("El número de página debe ser mayor a 0.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Validator_DebeFallar_CuandoPageSizeNoEsValido(int pageSize)
        {
            // Arramge
            var query = new GetHistorialPartidosQuery(1, pageSize);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(s => s.PageSize)
                .WithErrorMessage("El tamaño de página debe ser mayor a 0.");
        }
    }
}
