using MediatR;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;

namespace MundialitoCorp.Application.Features.Partidos.Queries.GetPartidoById
{
    public record GetPartidoByIdQuery(Guid Id) : IRequest<Result<PartidoDetalleReadModel>>;
}
