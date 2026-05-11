using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MundialitoCorp.Application.Features.Equipos.Commands.DeleteEquipo
{
    public class DeleteEquipoCommandHandler : IRequestHandler<DeleteEquipoCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEquipoRepository _equipoRepository;
        private readonly ILogger<DeleteEquipoCommandHandler> _logger;

        public DeleteEquipoCommandHandler(IUnitOfWork unitOfWork, IEquipoRepository equipoRepository, ILogger<DeleteEquipoCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _equipoRepository = equipoRepository;
            _logger = logger;
        }

        public async Task<Result> Handle(DeleteEquipoCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando la eliminación del equipo con Id: '{Id}'", request.Id);

            var equipo = await _equipoRepository.GetByIdAsync(request.Id);

            if (equipo == null)
            {
                _logger.LogError("Se intento eliminar un equipo inexistente con Id: '{Id}'", request.Id);
                return Result.Failure("El equipo que intentas eliminar no existe.", 404);
            }

            if (equipo.PartidosJugados > 0)
            {
                _logger.LogWarning("Se intentó eliminar un equipo que ya posee partidos jugados con Id: '{Id}'", request.Id);
                return Result.Failure("No se puede eliminar un equipo que ya ha disputado partidos.", 422);
            }

            await _equipoRepository.DeleteAsync(request.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Se eliminó el equipo correctamente con Id: '{Id}'", request.Id);

            return Result.Success(204);
        }
    }
}
