using MundialitoCorp.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MundialitoCorp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ApiControllerBase : ControllerBase
    {
        private ISender _mediator;

        protected ApiControllerBase(ISender mediator)
        {
            _mediator = mediator;
        }

        protected ISender Mediator => _mediator;

        protected ActionResult HandleResult(Result result)
        {
            var code = result.Code;
            if (result.Code == 204)
                code = 200;

            return StatusCode(code, result);
        }

        protected ActionResult HandleResult<T>(Result<T> result)
        {
            var code = result.Code;
            if (result.Code == 204)
                code = 200;

            return StatusCode(result.Code, result);
        }
    }
}