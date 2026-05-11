using MundialitoCorp.Application.Interfaces;
using MundialitoCorp.Domain.Repositories;
using MundialitoCorp.Infrastructure.Logging;
using MundialitoCorp.Infrastructure.Persistence;
using MundialitoCorp.Infrastructure.Persistence.Dapper;
using MundialitoCorp.Infrastructure.Persistence.Idempotency;
using MundialitoCorp.Infrastructure.Persistence.Queries;
using MundialitoCorp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MundialitoCorp.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IEquipoRepository, EquipoRepository>();
            services.AddScoped<IJugadorRepository, JugadorRepository>();
            services.AddScoped<IPartidoRepository, PartidoRepository>();

            services.AddScoped<IEquipoQueryService, EquipoQueryService>();
            services.AddScoped<IJugadorQueryService, JugadorQueryService>();
            services.AddScoped<IPartidoQueryService, PartidoQueryService>();
            services.AddScoped<IPosicionesTorneoQueryService, PosicionesTorneoQueryService>();
            services.AddScoped<IIdempotencyService, IdempotencyService>();

            services.AddSingleton<IDapperContext, DapperContext>();

            services.AddSingleton(typeof(EventLogger<>));
            services.AddScoped<MediatRDomainEventDispatcher>();

            return services;
        }
    }
}