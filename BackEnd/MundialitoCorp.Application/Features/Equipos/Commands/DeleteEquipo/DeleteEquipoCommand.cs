using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Equipos.Commands.DeleteEquipo
{
    public record DeleteEquipoCommand(Guid Id) : IRequest<Result>;
}
