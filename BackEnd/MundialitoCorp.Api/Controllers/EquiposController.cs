using MundialitoCorp.Api.Filters;
using MundialitoCorp.Application.Features.Equipos.Commands.CreateEquipo;
using MundialitoCorp.Application.Features.Equipos.Commands.DeleteEquipo;
using MundialitoCorp.Application.Features.Equipos.Commands.UpdateEquipo;
using MundialitoCorp.Application.Features.Equipos.Queries.GetAllEquipos;
using MundialitoCorp.Application.Features.Equipos.Queries.GetEquipoById;
using MundialitoCorp.Application.Features.Equipos.Queries.GetEquiposPaged;
using MundialitoCorp.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MundialitoCorp.Api.Controllers
{
    public class EquiposController : ApiControllerBase
    {
        public EquiposController(ISender mediator) : base(mediator)
        {
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPaged([FromQuery] GetEquiposPagedQuery query)
            => HandleResult(await Mediator.Send(query));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetEquipoByIdQuery(id);
            var result = await Mediator.Send(query);
            return HandleResult(result);
        }

        [HttpPost]
        [IdempotencyFilter]
        public async Task<IActionResult> Create([FromBody] CreateEquipoCommand command)
            => HandleResult(await Mediator.Send(command));

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEquipoCommand command)
        {
            if (id != command.Id)
                return HandleResult(Result.Failure("El ID de la URL no coincide con el del cuerpo.", 400));

            return HandleResult(await Mediator.Send(command));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
            => HandleResult(await Mediator.Send(new DeleteEquipoCommand(id)));

        [HttpGet("catalogo")]
        public async Task<IActionResult> GetCatalogoEquipo()
            => HandleResult(await Mediator.Send(new GetAllEquiposQuery()));
    }
}