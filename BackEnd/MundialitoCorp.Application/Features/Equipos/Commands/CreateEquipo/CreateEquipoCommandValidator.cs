using FluentValidation;

namespace MundialitoCorp.Application.Features.Equipos.Commands.CreateEquipo
{
    public class CreateEquipoCommandValidator : AbstractValidator<CreateEquipoCommand>
    {
        public CreateEquipoCommandValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del equipo no puede estar vacío.")
                .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");
        }
    }
}
