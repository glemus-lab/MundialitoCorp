using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Equipos.Commands.CreateEquipo
{
    public record CreateEquipoCommand(string Nombre) : IRequest<Result<Guid>>;
}
