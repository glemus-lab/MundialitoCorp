using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Jugadores.Commands.UpdateJugador
{
    public record UpdateJugadorCommand(Guid Id, string Nombre) : IRequest<Result>;
}
