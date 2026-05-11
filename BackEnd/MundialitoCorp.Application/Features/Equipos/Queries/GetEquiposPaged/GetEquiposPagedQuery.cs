using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Equipos.Queries.GetEquiposPaged
{
    public record GetEquiposPagedQuery(
            int PageNumber,
            int PageSize,
            string? SortBy,
            string? SortDirection,
            string? Filter
        ) : IRequest<Result<PagedList<EquipoPagedReadModel>>>;
}
