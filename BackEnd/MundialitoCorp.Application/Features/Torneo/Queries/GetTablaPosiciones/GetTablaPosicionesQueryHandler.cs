using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Torneo.Queries.GetTablaPosiciones
{
    public class GetTablaPosicionesQueryHandler : IRequestHandler<GetTablaPosicionesQuery, Result<IEnumerable<TablaPosicionesReadModel>>>
    {
        private readonly IPosicionesTorneoQueryService _tournamentQueryService;

        public GetTablaPosicionesQueryHandler(IPosicionesTorneoQueryService tournamentQueryService)
        {
            _tournamentQueryService = tournamentQueryService;
        }

        public async Task<Result<IEnumerable<TablaPosicionesReadModel>>> Handle(GetTablaPosicionesQuery request, CancellationToken cancellationToken)
        {
            var tabla = await _tournamentQueryService.GetTablaPosicionesAsync();
            return Result<IEnumerable<TablaPosicionesReadModel>>.Success(tabla, 200);
        }
    }
}
