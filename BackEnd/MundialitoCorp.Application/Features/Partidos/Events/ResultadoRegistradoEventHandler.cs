using MundialitoCorp.Domain.Events;
using MundialitoCorp.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MundialitoCorp.Application.Features.Partidos.Events
{
    public class ResultadoRegistradoEventHandler : INotificationHandler<ResultadoRegistradoEvent>
    {
        private readonly IEquipoRepository _equipoRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ResultadoRegistradoEventHandler> _logger;

        public ResultadoRegistradoEventHandler(
            IEquipoRepository equipoRepository,
            IUnitOfWork unitOfWork,
            ILogger<ResultadoRegistradoEventHandler> logger)
        {
            _equipoRepository = equipoRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handle(ResultadoRegistradoEvent notification, CancellationToken cancellationToken)
        {
            var local = await _equipoRepository.GetByIdAsync(notification.LocalId);
            var visitante = await _equipoRepository.GetByIdAsync(notification.VisitanteId);

            if (local == null || visitante == null)
            {
                _logger.LogWarning("[EVENTO DOMINIO] No se pudieron encontrar los equipos para el partido {PartidoId}", notification.PartidoId);
                throw new InvalidOperationException("No se pueden actualizar estadísticas: Uno de los equipos no existe.");
            }

            if (notification.GolesL < 0 || notification.GolesV < 0)
            {
                _logger.LogWarning("[EVENTO DOMINIO] No se pudieron actualizar las estadísticas de los euqipos del partido {PartidoId} porque lo goles son negativos", notification.PartidoId);
                throw new InvalidOperationException("No se pueden actualizar estadísticas: Los goles no pueden ser negativos.");
            }

            local.ActualizarEstadisticas(notification.GolesL, notification.GolesV);
            visitante.ActualizarEstadisticas(notification.GolesV, notification.GolesL);

            _equipoRepository.Update(local);
            _equipoRepository.Update(visitante);

            _logger.LogInformation("[EVENTO DOMINIO] Estadísticas actualizadas. Partido {PartidoId}: {Local} {GL} - {GV} {Visitante}",
                notification.PartidoId, local.Nombre, notification.GolesL, notification.GolesV, visitante.Nombre);
        }
    }
}