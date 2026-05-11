using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Partidos.Queries.GetPartidosPendientes
{
    public class GetPartidosPendientesQueryHandler : IRequestHandler<GetPartidosPendientesQuery, Result<List<PartidoReadModel>>>
    {
        private readonly IPartidoQueryService _queryService;

        public GetPartidosPendientesQueryHandler(IPartidoQueryService queryService)
        {
            _queryService = queryService;
        }

        public async Task<Result<List<PartidoReadModel>>> Handle(GetPartidosPendientesQuery request, CancellationToken ct)
        {
            var partidos = await _queryService.GetPartidosPendientesAsync();
            return Result<List<PartidoReadModel>>.Success(partidos.ToList(), 200);
        }
    }
}
