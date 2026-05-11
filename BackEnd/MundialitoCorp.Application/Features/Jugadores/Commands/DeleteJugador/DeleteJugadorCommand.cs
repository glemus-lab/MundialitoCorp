using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Jugadores.Commands.DeleteJugador
{
    public record DeleteJugadorCommand(Guid Id) : IRequest<Result>;
}
