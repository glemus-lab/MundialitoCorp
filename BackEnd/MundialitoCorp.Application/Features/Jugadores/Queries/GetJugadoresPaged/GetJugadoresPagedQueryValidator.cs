using MundialitoCorp.Application.Features.Jugadores.Queries.GetJugadoresPaged;
using FluentValidation;

namespace MundialitoCorp.Application.Features.Equipos.Queries.GetEquiposPaged
{
    public class GetJugadoresPagedQueryValidator : AbstractValidator<GetJugadoresPagedQuery>
    {
        public GetJugadoresPagedQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("El número de página debe ser mayor a 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("El tamaño de página debe ser mayor a 0.");
        }
    }
}
