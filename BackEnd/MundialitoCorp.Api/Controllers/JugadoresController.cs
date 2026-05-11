using MundialitoCorp.Api.Filters;
using MundialitoCorp.Application.Features.Jugadores.Commands.CreateJugador;
using MundialitoCorp.Application.Features.Jugadores.Commands.DeleteJugador;
using MundialitoCorp.Application.Features.Jugadores.Commands.UpdateJugador;
using MundialitoCorp.Application.Features.Jugadores.Queries.GetJugadorById;
using MundialitoCorp.Application.Features.Jugadores.Queries.GetJugadoresPaged;
using MundialitoCorp.Application.Features.Jugadores.Queries.GetRankingGoleadores;
using MundialitoCorp.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MundialitoCorp.Api.Controllers
{
    public class JugadoresController : ApiControllerBase
    {
        public JugadoresController(ISender mediator) : base(mediator)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPaged([FromQuery] GetJugadoresPagedQuery query)
            => HandleResult(await Mediator.Send(query));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetJugadorByIdQuery(id);
            return HandleResult(await Mediator.Send(query));
        }

        [HttpPost]
        [IdempotencyFilter]
        public async Task<IActionResult> Create([FromBody] CreateJugadorCommand command)
            => HandleResult(await Mediator.Send(command));

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateJugadorCommand command)
        {
            if (id != command.Id)
                return HandleResult(Result.Failure("El ID de la URL no coincide con el del cuerpo.", 400));
            
            return HandleResult(await Mediator.Send(command));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
            => HandleResult(await Mediator.Send(new DeleteJugadorCommand(id)));

        [HttpGet("ranking")]
        public async Task<IActionResult> GetRanking([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
            => HandleResult(await Mediator.Send(new GetRankingGoleadoresQuery(pageNumber, pageSize)));
    }
}