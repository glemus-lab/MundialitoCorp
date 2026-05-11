using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MundialitoCorp.Application.Features.Partidos.Commands.DeletePartido
{
    public class DeletePartidoCommandHandler : IRequestHandler<DeletePartidoCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPartidoRepository _partidoRepository;
        private readonly ILogger<DeletePartidoCommandHandler> _logger;

        public DeletePartidoCommandHandler(IUnitOfWork unitOfWork, IPartidoRepository partidoRepository, ILogger<DeletePartidoCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _partidoRepository = partidoRepository;
            _logger = logger;
        }

        public async Task<Result> Handle(DeletePartidoCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando la eliminación del partido con Id: '{Id}'.", request.Id);

            var partido = await _partidoRepository.GetByIdAsync(request.Id);

            if (partido is null)
            {
                _logger.LogError("El partido con Id: '{Id}' no existe.", request.Id);
                return Result.Failure("El partido que intentas eliminar no existe.", 404);
            }

            if (partido.EstaFinalizado)
            {
                _logger.LogWarning("El partido con Id: {Id} ya ha finalizado.", request.Id);
                return Result.Failure("No se puede eliminar un partido que ya ha finalizado.", 422);
            }

            await _partidoRepository.DeleteAsync(partido.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Se eliminó correctamente el partido con Id: '{Id}'.", request.Id);

            return Result.Success(204);
        }
    }
}
