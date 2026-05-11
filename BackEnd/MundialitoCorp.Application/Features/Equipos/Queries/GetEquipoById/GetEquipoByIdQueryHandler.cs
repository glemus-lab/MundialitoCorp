using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Equipos.Queries.GetEquipoById
{
    public class GetEquipoByIdQueryHandler : IRequestHandler<GetEquipoByIdQuery, Result<EquipoReadModel>>
    {
        private readonly IEquipoQueryService _queryService;

        public GetEquipoByIdQueryHandler(IEquipoQueryService queryService)
        {
            _queryService = queryService;
        }

        public async Task<Result<EquipoReadModel>> Handle(GetEquipoByIdQuery request, CancellationToken ct)
        {
            var equipo = await _queryService.GetByIdAsync(request.Id);

            if (equipo == null)
                return Result<EquipoReadModel>.Failure("El equipo solicitado no existe.", 404);

            return Result<EquipoReadModel>.Success(equipo, 200);
        }
    }
}
