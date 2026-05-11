using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Jugadores.Commands.CreateJugador
{
    public record CreateJugadorCommand(string Nombre, Guid EquipoId) : IRequest<Result<Guid>>;
}
