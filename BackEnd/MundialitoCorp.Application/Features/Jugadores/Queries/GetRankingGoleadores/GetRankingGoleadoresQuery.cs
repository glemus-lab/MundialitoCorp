using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Jugadores.Queries.GetRankingGoleadores
{
    public record GetRankingGoleadoresQuery(int PageNumber, int PageSize) : IRequest<Result<PagedList<JugadorReadModel>>>;
}
