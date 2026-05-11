using MundialitoCorp.Application.Features.Jugadores.Queries.GetRankingGoleadores;
using FluentValidation;

namespace MundialitoCorp.Application.Features.Equipos.Queries.GetEquiposPaged
{
    public class GetRankingGoleadoresQueryValidator : AbstractValidator<GetRankingGoleadoresQuery>
    {
        public GetRankingGoleadoresQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("El número de página debe ser mayor a 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("El tamaño de página debe ser mayor a 0.");
        }
    }
}
