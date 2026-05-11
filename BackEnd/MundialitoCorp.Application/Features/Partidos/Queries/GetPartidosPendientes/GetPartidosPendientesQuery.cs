using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Partidos.Queries.GetPartidosPendientes
{
    public record GetPartidosPendientesQuery() : IRequest<Result<List<PartidoReadModel>>>;
}
