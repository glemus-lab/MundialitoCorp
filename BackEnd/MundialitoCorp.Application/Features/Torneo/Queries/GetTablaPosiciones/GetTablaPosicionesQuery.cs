using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Torneo.Queries.GetTablaPosiciones
{
    public record GetTablaPosicionesQuery() : IRequest<Result<IEnumerable<TablaPosicionesReadModel>>>;
}
