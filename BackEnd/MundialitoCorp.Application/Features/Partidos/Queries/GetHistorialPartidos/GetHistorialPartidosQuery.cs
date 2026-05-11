using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Partidos.Queries.GetHistorialPartidos
{
    public record GetHistorialPartidosQuery(int PageNumber, int PageSize) : IRequest<Result<PagedList<PartidoReadModel>>>;
}
