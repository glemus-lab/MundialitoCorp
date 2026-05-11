using FluentValidation;

namespace MundialitoCorp.Application.Features.Partidos.Commands.UpdateFechaPartido
{
    public class UpdateFechaPartidoCommandValidator : AbstractValidator<UpdateFechaPartidoCommand>
    {
        public UpdateFechaPartidoCommandValidator()
        {
            RuleFor(x => x.NuevaFecha)
                .NotEmpty().WithMessage("La fecha del partido es obligatoria.");
        }
    }
}
