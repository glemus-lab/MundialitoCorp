using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Partidos.Queries.GetHistorialPartidos
{
    public class GetHistorialPartidosQueryHandler : IRequestHandler<GetHistorialPartidosQuery, Result<PagedList<PartidoReadModel>>>
    {
        private readonly IPartidoQueryService _queryService;

        public GetHistorialPartidosQueryHandler(IPartidoQueryService queryService)
        {
            _queryService = queryService;
        }

        public async Task<Result<PagedList<PartidoReadModel>>> Handle(GetHistorialPartidosQuery request, CancellationToken ct)
        {
            var pagedList = await _queryService.GetHistorialPartidosAsync(request.PageNumber, request.PageSize);
            return Result<PagedList<PartidoReadModel>>.Success(pagedList, 200);
        }
    }
}
