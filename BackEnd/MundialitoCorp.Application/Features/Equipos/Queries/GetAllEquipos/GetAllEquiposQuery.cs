using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Equipos.Queries.GetAllEquipos
{
    public record GetAllEquiposQuery() : IRequest<Result<IEnumerable<EquipoReadModel>>>;
}
