using MundialitoCorp.Application.Common;

namespace MundialitoCorp.Application.Interfaces
{
    public interface IIdempotencyService
    {
        Task<bool> RequestExistsAsync(Guid key);
        Task<IdempotencyResponse?> GetRequestAsync(Guid key);
        Task CreateRequestAsync(Guid key, string path, string responseBody, int statusCode);
    }
}
