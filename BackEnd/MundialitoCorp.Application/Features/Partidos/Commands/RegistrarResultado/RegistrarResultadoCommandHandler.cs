using MundialitoCorp.Domain.Common;
using MundialitoCorp.Domain.Repositories;
using MundialitoCorp.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;
using MundialitoCorp.Application.Interfaces;

namespace MundialitoCorp.Application.Features.Partidos.Commands.RegistrarResultado
{
    public class RegistrarResultadoCommandHandler : IRequestHandler<RegistrarResultadoCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPartidoRepository _partidoRepository;
        private readonly IJugadorQueryService _jugadorQueryService;
        private readonly ILogger<RegistrarResultadoCommandHandler> _logger;

        public RegistrarResultadoCommandHandler(
            IUnitOfWork unitOfWork,
            IPartidoRepository partidoRepository,
            IJugadorQueryService jugadorQueryService,
            ILogger<RegistrarResultadoCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _partidoRepository = partidoRepository;
            _jugadorQueryService = jugadorQueryService;
            _logger = logger;
        }

        public async Task<Result> Handle(RegistrarResultadoCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                _logger.LogInformation("Iniciando el registro de resultado Local: '{GolesLocal}' y Visitante: '{GolesVisitante}' para el partido con Id: {PartidoId}.", request.GolesLocal, request.GolesVisitante, request.PartidoId);

                if (request.GolesLocal < 0 || request.GolesVisitante < 0)
                {
                    var validationResult = new List<ValidationError>();

                    if (request.GolesLocal < 0)
                        validationResult.Add(new("GolesLocal", "Los goles del local no pueden ser negativos."));

                    if (request.GolesVisitante < 0)
                        validationResult.Add(new("GolesVisitante", "Los goles del visitante no pueden ser negativos."));

                    return Result.Failure("Error de validación.", 422, validationResult);
                }

                bool coincideGolesL = request.GolesLocal != request.GoleadoresLocalIds.Count;
                bool coincideGolesV = request.GolesVisitante != request.GoleadoresVisitanteIds.Count;
                if (coincideGolesL || coincideGolesV)
                {
                    var validationResult = new List<ValidationError>();

                    if (coincideGolesL)
                        validationResult.Add(new("GolesLocal", "Los goles del local deben coincidir con los goleadores"));

                    if (coincideGolesV)
                        validationResult.Add(new("GolesVisitante", "Los goles del visitante deben coincidir con los goleadores."));

                    return Result.Failure("Error de validación.", 422, validationResult);
                }

                var partido = await _partidoRepository.GetByIdAsync(request.PartidoId);
                if (partido == null)
                {
                    _logger.LogError("El partido con Id: {PartidoId} no existe.", request.PartidoId);
                    return Result.Failure("Partido no encontrado.", 404);
                }

                var todosLosGoleadores = request.GoleadoresLocalIds.Concat(request.GoleadoresVisitanteIds).ToList();

                if (!await _jugadorQueryService.ExistenTodosLosJugadoresAsync(todosLosGoleadores))
                {
                    return Result.Failure("Uno o más goleadores no son válidos o no existen.", 422);
                }

                var resultResultado = Resultado.Create(request.GolesLocal, request.GolesVisitante);

                if (resultResultado.IsFailure)
                {
                    _logger.LogError("No se pudo crear el Value Object 'Resultado' ErrorMessage: {ErrorMessage}", resultResultado.ErrorMessage);
                    return Result.Failure(resultResultado.ErrorMessage, resultResultado.Code, resultResultado.Errors);
                }

                var resultado = resultResultado.Value!;

                var resultPartido = partido.RegistrarResultado(
                    resultado,
                    request.GoleadoresLocalIds,
                    request.GoleadoresVisitanteIds);

                if (resultPartido.IsFailure)
                {
                    _logger.LogError("Error al registrar los resultados del partido con Id: '{PartidoId}'. ErrorMessage: '{ErrorMessage}'", request.PartidoId, resultPartido.ErrorMessage);
                    return resultPartido;
                }

                _partidoRepository.Update(partido);

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Se registro el resultado correctamente para el partido con Id: '{PartidoId}'", request.PartidoId);

                return Result.Success(200);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error en la transacción");
                return Result.Failure("Error interno al procesar la transacción.", 500);
            }
        }
    }
}