using FluentValidation;

namespace MundialitoCorp.Application.Features.Partidos.Commands.CreatePartido
{
    public class CreatePartidoCommandValidator : AbstractValidator<CreatePartidoCommand>
    {
        public CreatePartidoCommandValidator()
        {
            RuleFor(x => x.EquipoLocalId)
                .Must(id => id != null && id != Guid.Empty)
                .WithMessage("El equipo local es obligatorio.");

            RuleFor(x => x.EquipoVisitanteId)
                .Must(id => id != null && id != Guid.Empty).WithMessage("El equipo visitante es obligatorio.")
                .NotEqual(x => x.EquipoLocalId).WithMessage("Un equipo no puede jugar contra sí mismo.");

            RuleFor(x => x.Fecha)
                .NotEmpty().WithMessage("La fecha del partido es obligatoria.");
        }
    }
}
