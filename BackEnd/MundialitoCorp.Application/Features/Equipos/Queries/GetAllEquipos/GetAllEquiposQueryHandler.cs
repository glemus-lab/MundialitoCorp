using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Equipos.Queries.GetAllEquipos
{
    public class GetAllEquiposQueryHandler : IRequestHandler<GetAllEquiposQuery, Result<IEnumerable<EquipoReadModel>>>
    {

        private readonly IEquipoQueryService _equipoQueryService;

        public GetAllEquiposQueryHandler(IEquipoQueryService equipoQueryService)
        {
            _equipoQueryService = equipoQueryService;
        }

        public async Task<Result<IEnumerable<EquipoReadModel>>> Handle(GetAllEquiposQuery request, CancellationToken cancellationToken)
        {
            var equipos = await _equipoQueryService.GetAllAsync();
            return Result<IEnumerable<EquipoReadModel>>.Success(equipos, 200);
        }
    }
}
