using MediatR;

namespace MundialitoCorp.Domain.Events
{
    public record EquipoCreadoEvent(Guid EquipoId, string Nombre) : INotification;
}
