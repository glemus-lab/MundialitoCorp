using FluentValidation;

namespace MundialitoCorp.Application.Features.Jugadores.Commands.CreateJugador
{
    public class CreateJugadorCommandValidator : AbstractValidator<CreateJugadorCommand>
    {
        public CreateJugadorCommandValidator()
        {
            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del jugador es obligatorio.")
                .MaximumLength(150).WithMessage("El nombre no puede exceder los 150 caracteres.");

            RuleFor(x => x.EquipoId)
                .NotEmpty().WithMessage("El ID del equipo es obligatorio.");
        }
    }
}
