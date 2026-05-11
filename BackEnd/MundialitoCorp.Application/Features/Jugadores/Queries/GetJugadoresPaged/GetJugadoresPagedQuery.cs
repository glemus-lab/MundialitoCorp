using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Jugadores.Queries.GetJugadoresPaged
{
    public record GetJugadoresPagedQuery(int PageNumber, int PageSize, string? NombreFilter, Guid? EquipoId) : IRequest<Result<PagedList<JugadorReadModel>>>;
}
