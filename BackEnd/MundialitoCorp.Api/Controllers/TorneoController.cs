using MundialitoCorp.Application.Features.Torneo.Queries.GetTablaPosiciones;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MundialitoCorp.Api.Controllers
{
    [ApiController]
    [Route("api/torneo")]
    public class TorneoController : ApiControllerBase
    {
        public TorneoController(ISender mediator) : base(mediator)
        {
        }

        [HttpGet("tabla")]
        public async Task<IActionResult> GetTablaPosiciones()
            => Ok(await Mediator.Send(new GetTablaPosicionesQuery()));
    }
}