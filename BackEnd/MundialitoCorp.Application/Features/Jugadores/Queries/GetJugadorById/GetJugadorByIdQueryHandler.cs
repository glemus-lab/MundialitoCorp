using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Jugadores.Queries.GetJugadorById
{
    public class GetJugadorByIdQueryHandler : IRequestHandler<GetJugadorByIdQuery, Result<JugadorReadModel>>
    {
        private readonly IJugadorQueryService _jugadorQueryService;
        public GetJugadorByIdQueryHandler(IJugadorQueryService jugadorQueryService)
        {
            _jugadorQueryService = jugadorQueryService;
        }

        public async Task<Result<JugadorReadModel>> Handle(GetJugadorByIdQuery request, CancellationToken cancellationToken)
        {
            var jugador = await _jugadorQueryService.GetByIdAsync(request.Id);

            if (jugador == null)
                return Result<JugadorReadModel>.Failure("El jugador solicitado no existe.", 404);

            return Result<JugadorReadModel>.Success(jugador, 200);
        }
    }
}
