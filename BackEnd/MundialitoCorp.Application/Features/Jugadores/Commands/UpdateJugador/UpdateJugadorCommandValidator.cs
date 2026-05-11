using FluentValidation;

namespace MundialitoCorp.Application.Features.Jugadores.Commands.UpdateJugador
{
    public class UpdateJugadorCommandValidator : AbstractValidator<UpdateJugadorCommand>
    {
        public UpdateJugadorCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El ID del jugador es obligatorio.");

            RuleFor(x => x.Nombre)
                .NotEmpty().WithMessage("El nombre del jugador es obligatorio.")
                .MaximumLength(150).WithMessage("El nombre no puede exceder los 150 caracteres.");
        }
    }
}
