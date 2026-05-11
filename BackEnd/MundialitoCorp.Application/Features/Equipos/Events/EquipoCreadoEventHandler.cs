using MundialitoCorp.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MundialitoCorp.Application.Features.Equipos.Events
{
    public class EquipoCreadoEventHandler : INotificationHandler<EquipoCreadoEvent>
    {
        private readonly ILogger<EquipoCreadoEventHandler> _logger;

        public EquipoCreadoEventHandler(ILogger<EquipoCreadoEventHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(EquipoCreadoEvent notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[EVENTO DOMINIO] Nuevo equipo registrado: {Nombre} con ID: {Id}",
                notification.Nombre, notification.EquipoId);

            return Task.CompletedTask;
        }
    }
}
