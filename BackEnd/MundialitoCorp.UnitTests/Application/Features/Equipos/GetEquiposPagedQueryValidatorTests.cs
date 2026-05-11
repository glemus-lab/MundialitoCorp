using MundialitoCorp.Application.Features.Equipos.Queries.GetEquiposPaged;
using FluentValidation.TestHelper;

namespace MundialitoCorp.UnitTests.Application.Features.Equipos
{
    public class GetEquiposPagedQueryValidatorTests
    {
        private readonly GetEquiposPagedQueryValidator _validator = new();

        [Fact]
        public void Validator_DebePasar_CuandoQueryEsValido()
        {
            var query = new GetEquiposPagedQuery(1, 10, null, null, null);
            var result = _validator.TestValidate(query);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validator_DebeFallar_CuandoLaPaginaEsInvalida(int page)
        {
            var query = new GetEquiposPagedQuery(page, 10, null, null, null);
            var result = _validator.TestValidate(query);
            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                .WithErrorMessage("El número de página debe ser mayor a 0.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validator_DebeFallar_CuandoElTamanoEsInvalido(int size)
        {
            var query = new GetEquiposPagedQuery(1, size, null, null, null);
            var result = _validator.TestValidate(query);
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage("El tamaño de página debe ser mayor a 0.");
        }
    }
}