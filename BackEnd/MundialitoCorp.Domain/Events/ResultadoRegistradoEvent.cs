using MediatR;

namespace MundialitoCorp.Domain.Events
{
    public record ResultadoRegistradoEvent(Guid PartidoId, Guid LocalId, Guid VisitanteId, int GolesL, int GolesV, List<Guid> GoleadoresL, List<Guid> GoleadoresV) : INotification;
}
