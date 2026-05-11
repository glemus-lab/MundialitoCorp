using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Equipos.Queries.GetEquipoById
{
    public record GetEquipoByIdQuery(Guid Id) : IRequest<Result<EquipoReadModel>>;
}
