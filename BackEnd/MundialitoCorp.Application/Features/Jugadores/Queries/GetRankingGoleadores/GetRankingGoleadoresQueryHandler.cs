using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Jugadores.Queries.GetRankingGoleadores
{
    public class GetRankingGoleadoresQueryHandler : IRequestHandler<GetRankingGoleadoresQuery, Result<PagedList<JugadorReadModel>>>
    {
        private readonly IJugadorQueryService _queryService;

        public GetRankingGoleadoresQueryHandler(IJugadorQueryService queryService)
        {
            _queryService = queryService;
        }

        public async Task<Result<PagedList<JugadorReadModel>>> Handle(GetRankingGoleadoresQuery request, CancellationToken ct)
        {
            var ranking = await _queryService.GetRankingGoleadoresAsync(request.PageNumber, request.PageSize);
            return Result<PagedList<JugadorReadModel>>.Success(ranking, 200);
        }
    }
}
