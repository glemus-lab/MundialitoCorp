using MundialitoCorp.Api.Services;
using MundialitoCorp.Application.Interfaces;

namespace MundialitoCorp.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddWebApi(this IServiceCollection services)
        {
            services.AddScoped<ICorrelationIdGenerator, CorrelationIdGenerator>();

            return services;
        }
    }
}
