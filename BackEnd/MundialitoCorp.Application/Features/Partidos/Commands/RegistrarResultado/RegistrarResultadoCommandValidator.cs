using FluentValidation;

namespace MundialitoCorp.Application.Features.Partidos.Commands.RegistrarResultado
{
    public class RegistrarResultadoCommandValidator : AbstractValidator<RegistrarResultadoCommand>
    {
        public RegistrarResultadoCommandValidator()
        {
            RuleFor(x => x.PartidoId)
                .NotEmpty().WithMessage("El ID del partido es obligatorio.");

            RuleFor(x => x.GolesLocal)
                .GreaterThanOrEqualTo(0).WithMessage("Los goles no pueden ser negativos.");

            RuleFor(x => x.GolesVisitante)
                .GreaterThanOrEqualTo(0).WithMessage("Los goles no pueden ser negativos.");

            RuleFor(x => x.GoleadoresLocalIds)
                .Must((cmd, list) => list.Count == cmd.GolesLocal)
                .WithMessage("La lista de goleadores locales debe coincidir con el número de goles anotados.");

            RuleFor(x => x.GoleadoresVisitanteIds)
                .Must((cmd, list) => list.Count == cmd.GolesVisitante)
                .WithMessage("La lista de goleadores visitantes debe coincidir con el número de goles anotados.");
        }
    }
}
