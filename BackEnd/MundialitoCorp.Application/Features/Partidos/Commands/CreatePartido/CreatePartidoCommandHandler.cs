using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MundialitoCorp.Application.Features.Partidos.Commands.CreatePartido
{
    public class CreatePartidoCommandHandler : IRequestHandler<CreatePartidoCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPartidoRepository _partidoRepository;
        private readonly IPartidoQueryService _partidoQueryService;
        private readonly IEquipoRepository _equipoRepository;
        private readonly ILogger<CreatePartidoCommandHandler> _logger;

        public CreatePartidoCommandHandler(IUnitOfWork unitOfWork, IPartidoRepository partidoRepository, IEquipoRepository equipoRepository, IPartidoQueryService partidoQueryService, ILogger<CreatePartidoCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _partidoRepository = partidoRepository;
            _equipoRepository = equipoRepository;
            _partidoQueryService = partidoQueryService;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(CreatePartidoCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando la creación del partido entre el equipo local con Id '{EquipoLocalId}' y el visitante '{EquipoVisitanteId}' el día '{Fecha}'.", request.EquipoLocalId, request.EquipoVisitanteId, request.Fecha.HasValue ? request.Fecha.Value.ToString("dd/MM/yyyy") : null);

            var local = await _equipoRepository.GetByIdAsync(request.EquipoLocalId!.Value);
            var visitante = await _equipoRepository.GetByIdAsync(request.EquipoVisitanteId!.Value);

            if (local is null || visitante is null)
            {
                var validationError = new List<ValidationError>();

                if (local is null)
                {
                    _logger.LogWarning("El equipo local con Id: '{EquipoLocalId}' no existe.", request.EquipoLocalId);
                    validationError.Add(new("EquipoLocalId", "El equipo no está registrado."));
                }

                if (visitante is null)
                {
                    _logger.LogWarning("El equipo visitante con Id: '{EquipoVisitanteId}' no existe.", request.EquipoVisitanteId);
                    validationError.Add(new("EquipoVisitanteId", "El equipo no está registrado."));
                }
                
                return Result<Guid>.Failure("No se pudo crear el partido.", 422, validationError);
            }

            bool conflictoLocal = await _partidoQueryService.ExisteConflictoFechaAsync(request.EquipoLocalId.Value, request.Fecha!.Value);
            bool conflictoVis = await _partidoQueryService.ExisteConflictoFechaAsync(request.EquipoVisitanteId!.Value, request.Fecha!.Value);

            if (conflictoLocal || conflictoVis)
            {
                var validationError = new List<ValidationError>();

                if (conflictoLocal)
                {
                    _logger.LogWarning("El equipo local con Id: '{EquipoLocalId}' ya tiene un compromiso el dia '{Fecha}'.", request.EquipoLocalId, request.Fecha.HasValue ? request.Fecha.Value.ToString("dd/MM/yyyy") : string.Empty);
                    validationError.Add(new ValidationError("EquipoLocalId", "El equipo ya tiene un partido programado en esta fecha."));
                }

                if (conflictoVis)
                {
                    _logger.LogWarning("El equipo visitante con Id: '{EquipoVisitanteId}' ya tiene un compromiso el dia '{Fecha}'.", request.EquipoVisitanteId, request.Fecha.HasValue ? request.Fecha.Value.ToString("dd/MM/yyyy") : string.Empty);
                    validationError.Add(new ValidationError("EquipoVisitanteId", "El equipo ya tiene un partido programado en esta fecha."));
                }

                return Result<Guid>.Failure("No se pudo crear el partido.", 422, validationError);
            }

            var result = Partido.Create(request.EquipoLocalId!.Value, request.EquipoVisitanteId!.Value, request.Fecha!.Value);

            if (result.IsFailure)
                return Result<Guid>.Failure(result.ErrorMessage, result.Code, result.Errors);

            var partido = result.Value!;

            await _partidoRepository.AddAsync(partido);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Se creó el partido correctamente con Id: '{Id}' entre el equipo '{NombreEquipoLocal}' y '{NombreEquipoVisitante}' para el día '{Fecha}'.", partido.Id, local.Nombre, visitante.Nombre, partido.Fecha.ToString("dd/MM/yyyy"));

            return Result<Guid>.Success(partido.Id, 201);
        }
    }
}
