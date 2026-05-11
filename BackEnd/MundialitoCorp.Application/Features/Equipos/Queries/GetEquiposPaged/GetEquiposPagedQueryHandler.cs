using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Equipos.Queries.GetEquiposPaged
{
    public class GetEquiposPagedQueryHandler : IRequestHandler<GetEquiposPagedQuery, Result<PagedList<EquipoPagedReadModel>>>
    {
        private readonly IEquipoQueryService _equipoQueryService;

        public GetEquiposPagedQueryHandler(IEquipoQueryService equipoQueryService)
        {
            _equipoQueryService = equipoQueryService;
        }

        public async Task<Result<PagedList<EquipoPagedReadModel>>> Handle(GetEquiposPagedQuery request, CancellationToken cancellationToken)
        {
            var pagedList = await _equipoQueryService.GetEquiposPagedAsync(request.PageNumber, request.PageSize, request.SortBy, request.SortDirection, request.Filter);
            return Result<PagedList<EquipoPagedReadModel>>.Success(pagedList, 200);
        }
    }
}
