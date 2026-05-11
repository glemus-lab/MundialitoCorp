using MundialitoCorp.Application.Interfaces;

namespace MundialitoCorp.Api.Services
{
    public class CorrelationIdGenerator : ICorrelationIdGenerator
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CorrelationIdGenerator(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string Get()
        {
            return _httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString() ?? "System";
        }
    }
}
