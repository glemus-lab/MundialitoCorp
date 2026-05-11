using MundialitoCorp.Application.Models;
using MundialitoCorp.Domain.Common;
using MediatR;

namespace MundialitoCorp.Application.Features.Jugadores.Queries.GetJugadorById
{
    public record GetJugadorByIdQuery(Guid Id) : IRequest<Result<JugadorReadModel>>;
}
