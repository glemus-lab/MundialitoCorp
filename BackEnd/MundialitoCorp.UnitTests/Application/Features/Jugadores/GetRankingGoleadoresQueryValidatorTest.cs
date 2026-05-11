using MundialitoCorp.Application.Features.Equipos.Queries.GetEquiposPaged;
using MundialitoCorp.Application.Features.Jugadores.Queries.GetRankingGoleadores;
using FluentValidation.TestHelper;

namespace MundialitoCorp.UnitTests.Application.Features.Jugadores
{
    public class GetRankingGoleadoresQueryValidatorTest
    {
        private readonly GetRankingGoleadoresQueryValidator _validator;

        public GetRankingGoleadoresQueryValidatorTest()
        {
            _validator = new GetRankingGoleadoresQueryValidator();
        }

        [Fact]
        public async Task Handle_DebePasar_CuandoQueryEsValido()
        {
            // Arrange
            var query = new GetRankingGoleadoresQuery(1, 5);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Handle_DebeFallar_CuandoPageNumberNoEsValido(int pageNumber)
        {
            // Arrange
            var query = new GetRankingGoleadoresQuery(pageNumber, 5);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(s => s.PageNumber)
                .WithErrorMessage("El número de página debe ser mayor a 0.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Handle_DebeFallar_CuandoPageSizeNoEsValido(int pageSize)
        {
            // Arrange
            var query = new GetRankingGoleadoresQuery(1, pageSize);

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(s => s.PageSize)
                .WithErrorMessage("El tamaño de página debe ser mayor a 0.");
        }
    }
}
