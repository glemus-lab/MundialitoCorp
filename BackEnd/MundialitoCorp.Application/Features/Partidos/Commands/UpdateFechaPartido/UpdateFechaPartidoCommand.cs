using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Partidos.Commands.UpdateFechaPartido
{
    public record UpdateFechaPartidoCommand(Guid Id, DateOnly NuevaFecha) : IRequest<Result>;
}
