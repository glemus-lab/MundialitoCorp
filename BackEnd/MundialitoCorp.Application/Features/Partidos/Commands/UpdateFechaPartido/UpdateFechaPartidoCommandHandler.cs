using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MundialitoCorp.Application.Features.Partidos.Commands.UpdateFechaPartido
{
    public class UpdateFechaPartidoCommandHandler : IRequestHandler<UpdateFechaPartidoCommand, Result>
    {
        private readonly IPartidoRepository _repository;
        private readonly IPartidoQueryService _queryService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateFechaPartidoCommandHandler> _logger;

        public UpdateFechaPartidoCommandHandler(IPartidoRepository repository, IPartidoQueryService queryService, IUnitOfWork unitOfWork, ILogger<UpdateFechaPartidoCommandHandler> logger)
        {
            _repository = repository;
            _queryService = queryService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Result> Handle(UpdateFechaPartidoCommand request, CancellationToken ct)
        {
            _logger.LogInformation("Iniciando la actualización de la fecha para el partido con Id: '{Id}'.", request.Id);

            var partido = await _repository.GetByIdAsync(request.Id);
            if (partido == null)
            {
                _logger.LogWarning("El partido con Id: '{Id}' no existe.", request.Id);
                return Result.Failure("El partido no existe.", 404);
            }
            
            if (partido.EstaFinalizado)
            {
                _logger.LogWarning("El partido con Id: '{Id} ya ha sido finalizado.'", request.Id);
                return Result.Failure("No se puede cambiar la fecha de un partido finalizado.", 422);
            }

            if (partido.Fecha == request.NuevaFecha)
                return Result.Success(200);

            var tieneConflictoLocal = await _queryService.ExisteConflictoFechaAsync(partido.EquipoLocalId, request.NuevaFecha);
            var tieneConflictoVisitante = await _queryService.ExisteConflictoFechaAsync(partido.EquipoVisitanteId, request.NuevaFecha);

            if (tieneConflictoLocal || tieneConflictoVisitante)
            {
                if (tieneConflictoLocal)
                    _logger.LogWarning("El equipo con Id: '{EquipoLocalId}' ya tiene compromiso para el dia '{NuevaFecha}'.", partido.EquipoLocalId, request.NuevaFecha.ToString("dd/MM/yyyy"));

                if (tieneConflictoVisitante)
                    _logger.LogWarning("El equipo con Id: '{EquipoVisitanteId}' ya tiene compromiso para el dia '{NuevaFecha}'.", partido.EquipoVisitanteId, request.NuevaFecha.ToString("dd/MM/yyyy"));

                return Result.Failure("Uno de los equipos ya tiene un compromiso en esa fecha.", 422);
            }

            _logger.LogInformation("Cambiando la fecha del partido con Id: '{Id}' de '{FechaActual}' a '{NuevaFecha}'.", request.Id, partido.Fecha.ToString("dd/MM/yyyy"), request.NuevaFecha.ToString("dd/MM/yyyy"));
            
            var result = partido.UpdateFecha(request.NuevaFecha);
            if (result.IsFailure)
            {
                _logger.LogError("Error al actualizar la fecha para el partido con Id: '{Id}'.", request.Id);
                return result;
            }

            _repository.Update(partido);

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("Se actualizo correctamente la fecha '{NuevaFecha}' para el partido con Id: '{Id}'.", request.NuevaFecha.ToString("dd/MM/yyyy"), request.Id);

            return Result.Success(200);
        }
    }
}
