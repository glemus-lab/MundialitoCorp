using FluentValidation;

namespace MundialitoCorp.Application.Features.Equipos.Commands.UpdateEquipo
{
    public class UpdateEquipoCommandValidator : AbstractValidator<UpdateEquipoCommand>
    {
        public UpdateEquipoCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El ID del equipo es obligatorio.");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del equipo es obligatorio.")
                .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");
        }
    }
}
