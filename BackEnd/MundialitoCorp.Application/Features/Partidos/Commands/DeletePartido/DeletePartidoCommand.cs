using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Partidos.Commands.DeletePartido
{
    public record DeletePartidoCommand(Guid Id) : IRequest<Result>;
}
