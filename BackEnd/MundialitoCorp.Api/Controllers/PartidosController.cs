using MediatR;
using Microsoft.AspNetCore.Mvc;
using MundialitoCorp.Api.Filters;
using MundialitoCorp.Application.Features.Partidos.Commands.CreatePartido;
using MundialitoCorp.Application.Features.Partidos.Commands.DeletePartido;
using MundialitoCorp.Application.Features.Partidos.Commands.RegistrarResultado;
using MundialitoCorp.Application.Features.Partidos.Commands.UpdateFechaPartido;
using MundialitoCorp.Application.Features.Partidos.Queries.GetHistorialPartidos;
using MundialitoCorp.Application.Features.Partidos.Queries.GetPartidoById;
using MundialitoCorp.Application.Features.Partidos.Queries.GetPartidosPendientes;
using MundialitoCorp.Domain.Common;

namespace MundialitoCorp.Api.Controllers
{
    public class PartidosController : ApiControllerBase
    {

        public PartidosController(ISender mediator) : base(mediator)
        {
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
            => HandleResult(await Mediator.Send(new GetPartidoByIdQuery(id)));

        [HttpPost]
        [IdempotencyFilter]
        public async Task<IActionResult> Create([FromBody] CreatePartidoCommand command)
            => HandleResult(await Mediator.Send(command));

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
            => HandleResult(await Mediator.Send(new DeletePartidoCommand(id)));

        [HttpPatch("registrar-resultado")]
        public async Task<IActionResult> RegistrarResultado([FromBody] RegistrarResultadoCommand command)
            => HandleResult(await Mediator.Send(command));

        [HttpPatch("{id}/fecha")]
        public async Task<IActionResult> UpdateFecha(Guid id, [FromBody] UpdateFechaPartidoCommand command)
        {
            if (id != command.Id)
                return HandleResult(Result.Failure("El ID de la URL no coincide con el del cuerpo.", 400));

            return HandleResult(await Mediator.Send(command));
        }

        [HttpGet("pendientes")]
        public async Task<IActionResult> GetPendientes()
            => HandleResult(await Mediator.Send(new GetPartidosPendientesQuery()));

        [HttpGet("historial")]
        public async Task<IActionResult> GetHistorial([FromQuery] GetHistorialPartidosQuery query)
            => HandleResult(await Mediator.Send(query));
    }
}