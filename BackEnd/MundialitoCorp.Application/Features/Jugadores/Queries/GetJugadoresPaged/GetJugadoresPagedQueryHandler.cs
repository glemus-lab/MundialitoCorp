using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Jugadores.Queries.GetJugadoresPaged
{
    public class GetJugadoresPagedQueryHandler : IRequestHandler<GetJugadoresPagedQuery, Result<PagedList<JugadorReadModel>>>
    {
        private readonly IJugadorQueryService _jugadorQueryService;

        public GetJugadoresPagedQueryHandler(IJugadorQueryService jugadorQueryService)
        {
            _jugadorQueryService = jugadorQueryService;
        }

        public async Task<Result<PagedList<JugadorReadModel>>> Handle(GetJugadoresPagedQuery request, CancellationToken cancellationToken)
        {
            var jugadores = await _jugadorQueryService.GetJugadoresPagedAsync(request.PageNumber, request.PageSize, request.NombreFilter, request.EquipoId);
            return Result<PagedList<JugadorReadModel>>.Success(jugadores, 200);
        }
    }
}
