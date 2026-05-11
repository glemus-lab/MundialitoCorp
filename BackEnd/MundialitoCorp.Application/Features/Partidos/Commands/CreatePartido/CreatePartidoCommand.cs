using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Partidos.Commands.CreatePartido
{
    public record CreatePartidoCommand(Guid? EquipoLocalId, Guid? EquipoVisitanteId, DateOnly? Fecha) : IRequest<Result<Guid>>;
}
