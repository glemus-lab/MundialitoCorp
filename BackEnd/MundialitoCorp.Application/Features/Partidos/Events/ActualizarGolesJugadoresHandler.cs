using MundialitoCorp.Domain.Events;
using MundialitoCorp.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MundialitoCorp.Application.Features.Partidos.Events
{
    public class ActualizarGolesJugadoresHandler : INotificationHandler<ResultadoRegistradoEvent>
    {
        private readonly IJugadorRepository _jugadorRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ActualizarGolesJugadoresHandler> _logger;

        public ActualizarGolesJugadoresHandler(
            IJugadorRepository jugadorRepository,
            IUnitOfWork unitOfWork,
            ILogger<ActualizarGolesJugadoresHandler> logger)
        {
            _jugadorRepository = jugadorRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Handle(ResultadoRegistradoEvent notification, CancellationToken cancellationToken)
        {
            var todosLosGoleadoresIds = notification.GoleadoresL.Concat(notification.GoleadoresV).ToList();

            if (!todosLosGoleadoresIds.Any()) return;

            foreach (var id in todosLosGoleadoresIds)
            {
                var jugador = await _jugadorRepository.GetByIdAsync(id);

                if (jugador != null)
                {
                    jugador.RegistrarGol();
                    _jugadorRepository.Update(jugador);

                    _logger.LogInformation("[EVENTO DOMINIO] Gol registrado para el jugador: {Nombre}", jugador.Nombre);
                }
            }
        }
    }
}
