namespace MundialitoCorp.Application.Common
{
    public record IdempotencyResponse(Guid Key, string ResponseBody, int StatusCode);
}
