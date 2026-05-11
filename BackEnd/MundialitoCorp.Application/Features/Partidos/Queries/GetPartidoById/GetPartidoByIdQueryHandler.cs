using MediatR;
using MundialitoCorp.Application.Features.Partidos.Queries.GetPartidoById;
using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;

namespace ControlTorneoFootball.Application.Features.Partidos.Queries.GetPartidoById
{
    public class GetPartidoByIdQueryHandler : IRequestHandler<GetPartidoByIdQuery, Result<PartidoDetalleReadModel>>
    {
        private readonly IPartidoQueryService _partidoQueryService;

        public GetPartidoByIdQueryHandler(IPartidoQueryService partidoQueryService)
        {
            _partidoQueryService = partidoQueryService;
        }

        public async Task<Result<PartidoDetalleReadModel>> Handle(GetPartidoByIdQuery request, CancellationToken cancellationToken)
        {
            var partido = await _partidoQueryService.GetByIdAsync(request.Id);
            
            if (partido is null)
                return Result<PartidoDetalleReadModel>.Failure("El partido solicitado no existe.", 404);

            return Result<PartidoDetalleReadModel>.Success(partido, 200);
        }
    }
}
