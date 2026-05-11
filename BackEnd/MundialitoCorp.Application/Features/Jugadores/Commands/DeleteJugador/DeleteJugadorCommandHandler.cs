using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MundialitoCorp.Application.Features.Jugadores.Commands.DeleteJugador
{
    public class DeleteJugadorCommandHandler : IRequestHandler<DeleteJugadorCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJugadorRepository _jugadorRepository;
        private readonly ILogger<DeleteJugadorCommandHandler> _logger;

        public DeleteJugadorCommandHandler(IUnitOfWork unitOfWork, IJugadorRepository jugadorRepository, ILogger<DeleteJugadorCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _jugadorRepository = jugadorRepository;
            _logger = logger;
        }

        public async Task<Result> Handle(DeleteJugadorCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando la eiliminación del jugador con Id: '{Id}'", request.Id);

            var jugador = await _jugadorRepository.GetByIdAsync(request.Id);
            
            if (jugador == null) 
            {
                _logger.LogError("El jugador con Id: '{Id}' no existe.", request.Id);
                return Result.Failure("Jugador no encontrado.", 404);
            }

            await _jugadorRepository.DeleteAsync(request.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Se eliminó correctamente al jugador con Id: '{Id}'", request.Id);

            return Result.Success(204);
        }
    }
}
