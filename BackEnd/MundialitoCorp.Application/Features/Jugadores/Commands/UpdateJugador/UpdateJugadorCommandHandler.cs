using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MundialitoCorp.Application.Features.Jugadores.Commands.UpdateJugador
{
    public class UpdateJugadorCommandHandler : IRequestHandler<UpdateJugadorCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJugadorRepository _jugadorRepository;
        private readonly ILogger<UpdateJugadorCommandHandler> _logger;

        public UpdateJugadorCommandHandler(IUnitOfWork unitOfWork, IJugadorRepository jugadorRepository, ILogger<UpdateJugadorCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _jugadorRepository = jugadorRepository;
            _logger = logger;
        }

        public async Task<Result> Handle(UpdateJugadorCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando la actualización del jugador con Id: '{Id}' al nuevo nombre '{Nombre}'.", request.Id, request.Nombre);

            var jugador = await _jugadorRepository.GetByIdAsync(request.Id);
            if (jugador == null)
            {
                _logger.LogError("El jugado con Id: '{Id}' no existe.", request.Id);
                return Result.Failure("Jugador no encontrado.", 404);
            }

            _logger.LogInformation("Cambiando nombre del jugador con Id: '{Id}' de '{NombreActual}' a '{NombreNuevo}'.", request.Id, jugador.Nombre, request.Nombre);
            var result = jugador.CambiarNombre(request.Nombre);

            if (result.IsFailure)
            {
                _logger.LogInformation("Error al cambiar el nombre del jugador. ErrorMessage: '{ErrorMessage}'.", result.ErrorMessage);
                return result;
            }

            _jugadorRepository.Update(jugador);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Se actualizó crrectamente el nombre del jugador con Id: '{Id}'.", request.Id);

            return Result.Success(200);
        }
    }
}
