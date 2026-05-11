using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Partidos.Commands.RegistrarResultado
{
    public record RegistrarResultadoCommand(
            Guid PartidoId,
            int GolesLocal,
            int GolesVisitante,
            List<Guid> GoleadoresLocalIds,
            List<Guid> GoleadoresVisitanteIds
        ) : IRequest<Result>;
}
