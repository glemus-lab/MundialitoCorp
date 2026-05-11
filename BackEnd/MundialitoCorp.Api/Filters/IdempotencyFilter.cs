using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace MundialitoCorp.Api.Filters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class IdempotencyFilterAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue("Idempotency-Key", out var extractedKey))
            {
                var result = Result.Failure("El header 'Idempotency-Key' es obligatorio.", 400);
                context.Result = new BadRequestObjectResult(result);
                return;
            }

            if (!Guid.TryParse(extractedKey, out var key))
            {
                var resultInvalid = Result.Failure("Formato de Idempotency-Key inválido.", 400);
                context.Result = new BadRequestObjectResult(resultInvalid);
                return;
            }

            var idempotencyService = context.HttpContext.RequestServices.GetRequiredService<IIdempotencyService>();

            var existingRequest = await idempotencyService.GetRequestAsync(key);
            if (existingRequest != null)
            {
                context.Result = new ContentResult
                {
                    Content = existingRequest.ResponseBody,
                    ContentType = "application/json",
                    StatusCode = existingRequest.StatusCode
                };
                return;
            }

            try
            {
                var executedContext = await next();

                if (executedContext.Result is ObjectResult objectResult &&
                    objectResult.StatusCode >= 200 && objectResult.StatusCode < 300)
                {
                    var responseBody = System.Text.Json.JsonSerializer.Serialize(objectResult.Value);
                    await idempotencyService.CreateRequestAsync(key, context.ActionDescriptor.DisplayName!, responseBody, objectResult.StatusCode.Value);
                }
            }
            catch (Exception ex) when (IsUniqueConstraintViolation(ex))
            {
                await Task.Delay(200);

                var retryRequest = await idempotencyService.GetRequestAsync(key);
                if (retryRequest != null)
                {
                    context.Result = new ContentResult
                    {
                        Content = retryRequest.ResponseBody,
                        ContentType = "application/json",
                        StatusCode = retryRequest.StatusCode
                    };
                }
                else
                {
                    var conflictResult = Result.Failure("La petición ya está siendo procesada o hubo un conflicto.", 409);
                    context.Result = new ConflictObjectResult(conflictResult);
                }
            }
        }

        private bool IsUniqueConstraintViolation(Exception ex)
        {
            if (ex is DbUpdateException dbEx && dbEx.InnerException is SqlException sqlEx)
            {
                return sqlEx.Number == 2627 || sqlEx.Number == 2601;
            }
            return false;
        }
    }
}