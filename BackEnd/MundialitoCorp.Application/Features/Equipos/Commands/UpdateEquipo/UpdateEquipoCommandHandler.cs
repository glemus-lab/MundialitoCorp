using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MundialitoCorp.Application.Features.Equipos.Commands.UpdateEquipo
{
    public class UpdateEquipoCommandHandler : IRequestHandler<UpdateEquipoCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEquipoRepository _equipoRepository;
        private readonly ILogger<UpdateEquipoCommandHandler> _logger;

        public UpdateEquipoCommandHandler(IUnitOfWork unitOfWork, IEquipoRepository equipoRepository, ILogger<UpdateEquipoCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _equipoRepository = equipoRepository;
            _logger = logger;
        }

        public async Task<Result> Handle(UpdateEquipoCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando la actualización del equipo con Id: '{Id}' y el nuevo nombre '{Nombre}'", request.Id, request.Nombre);

            var equipo = await _equipoRepository.GetByIdAsync(request.Id);

            if (equipo == null)
            {
                _logger.LogError("Se intentó actualizar un equipo inexistente con Id: '{Id}'", request.Id);
                return Result.Failure("El equipo que intentas actualizar no existe.", 404);
            }

            var existeNombre = await _equipoRepository.ExistsAsync(request.Nombre);
            
            if (existeNombre && !string.Equals(equipo.Nombre, request.Nombre, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Duplicidad de nombre para el equipo con Id: '{Id}' y el nombre '{Nombre}'", request.Id, request.Nombre);
                return Result.Failure("Error de validación", 409, [new("Nombre", "Ya existe otro equipo registrado con ese nombre.")]);
            }

            _logger.LogInformation("Cambiando el nombre del equipo con Id: '{Id}' de '{NombreActual}' a '{NombreNuevo}'.", request.Id, equipo.Nombre, request.Nombre);
            var result = equipo.CambiarNombre(request.Nombre);

            if (result.IsFailure)
            {
                _logger.LogWarning("Error al actualizar el nombre del equipo. ErrorMessage: '{ErrorMessage}'.", result.ErrorMessage);
                return result;
            }

            _equipoRepository.Update(equipo);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Se actualizó correctamente el equipo con Id: '{Id}' y el nombre '{Nombre}'.", request.Id, request.Nombre);

            return Result.Success(200);
        }
    }
}
