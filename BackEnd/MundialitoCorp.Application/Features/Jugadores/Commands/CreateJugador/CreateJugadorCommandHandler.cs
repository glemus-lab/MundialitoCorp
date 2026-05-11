using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MundialitoCorp.Application.Features.Jugadores.Commands.CreateJugador
{
    public class CreateJugadorCommandHandler : IRequestHandler<CreateJugadorCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEquipoRepository _equipoRepository;
        private readonly ILogger<CreateJugadorCommandHandler> _logger;

        public CreateJugadorCommandHandler(IUnitOfWork unitOfWork, IEquipoRepository equipoRepository, ILogger<CreateJugadorCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _equipoRepository = equipoRepository;
            _logger = logger;
        }

        public async Task<Result<Guid>> Handle(CreateJugadorCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Iniciando la creacion del jugador con el nombre '{Nombre}' para el equipo con EquipoId: '{EquipoId}'", request.Nombre, request.EquipoId);

            var equipo = await _equipoRepository.GetByIdAsync(request.EquipoId);
            if (equipo == null)
            {
                _logger.LogWarning("El equipo con Id: '{EquipoId}' no existe y no se puede agregar el jugador con nombre '{Nombre}'.", request.EquipoId, request.Nombre);
                return Result<Guid>.Failure("El equipo destino no existe.", 404);
            }

            var result = equipo.AgregarJugador(request.Nombre);

            if (result.IsFailure)
            {
                _logger.LogInformation("Error al agregar el jugador al equipo '{ErrorMessage}'", result.ErrorMessage);
                return Result<Guid>.Failure(result.ErrorMessage, result.Code, result.Errors); 
            }
                        
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var jugadorCreado = equipo.Jugadores.Last();

            _logger.LogInformation("Se agrego correctamente al jugador '{Nombre}' con Id: '{Id}' al equipo", request.Nombre, jugadorCreado.Id);

            return Result<Guid>.Success(jugadorCreado.Id, 201);
        }
    }
}
