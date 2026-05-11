using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Equipos.Commands.UpdateEquipo
{
    public record UpdateEquipoCommand(Guid Id, string Nombre) : IRequest<Result>;
}
