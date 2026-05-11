using MundialitoCorp.Application.Common;
using MundialitoCorp.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MundialitoCorp.Infrastructure.Persistence.Idempotency
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly ApplicationDbContext _context;

        public IdempotencyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RequestExistsAsync(Guid key)
        {
            return await _context.Set<IdempotencyKey>().AnyAsync(x => x.Key == key);
        }

        public async Task<IdempotencyResponse?> GetRequestAsync(Guid key)
        {
            var idempotency = await _context.Set<IdempotencyKey>().FirstOrDefaultAsync(x => x.Key == key);

            if (idempotency == null)
                return null;

            return new IdempotencyResponse(idempotency.Key, idempotency.ResponseBody, idempotency.StatusCode);
        }

        public async Task CreateRequestAsync(Guid key, string path, string responseBody, int statusCode)
        {
            var idempotencyKey = new IdempotencyKey
            {
                Key = key,
                RequestPath = path,
                ResponseBody = responseBody,
                StatusCode = statusCode,
                CreatedAt = DateTime.Now
            };

            _context.IdempotencyKeys.Add(idempotencyKey);
            await _context.SaveChangesAsync();
        }
    }
}
