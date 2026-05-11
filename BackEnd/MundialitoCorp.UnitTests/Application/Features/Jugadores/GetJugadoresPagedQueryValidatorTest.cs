using MundialitoCorp.Application.Features.Equipos.Queries.GetEquiposPaged;
using MundialitoCorp.Application.Features.Jugadores.Queries.GetJugadoresPaged;
using FluentValidation.TestHelper;

namespace MundialitoCorp.UnitTests.Application.Features.Jugadores
{
    public class GetJugadoresPagedQueryValidatorTest
    {
        private readonly GetJugadoresPagedQueryValidator _validator;

        public GetJugadoresPagedQueryValidatorTest()
        {
            _validator = new GetJugadoresPagedQueryValidator();
        }

        [Fact]
        public async Task Handler_DebePasar_CuandoElQueryEsValido()
        {
            // Arrange
            var query = new GetJugadoresPagedQuery(1, 5, "Nombre Jugador", Guid.NewGuid());

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Handler_DebeFallar_CuandoPageNumberNoEsValido(int pageNumber)
        {
            // Arrange
            var query = new GetJugadoresPagedQuery(pageNumber, 5, "Nombre Jugador", Guid.NewGuid());

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(s => s.PageNumber)
                .WithErrorMessage("El número de página debe ser mayor a 0.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Handler_DebeFallar_CuandoPageSizeNoEsValido(int pageSize)
        {
            // Arrange
            var query = new GetJugadoresPagedQuery(1, pageSize, "Nombre Jugador", Guid.NewGuid());

            // Act
            var result = _validator.TestValidate(query);

            // Assert
            result.ShouldHaveValidationErrorFor(s => s.PageSize)
                .WithErrorMessage("El tamaño de página debe ser mayor a 0.");
        }
    }
}
