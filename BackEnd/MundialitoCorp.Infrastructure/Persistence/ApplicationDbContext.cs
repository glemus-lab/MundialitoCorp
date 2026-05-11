using System.Reflection;
using MundialitoCorp.Domain.Entities;
using MundialitoCorp.Infrastructure.Persistence.Idempotency;
using Microsoft.EntityFrameworkCore;

namespace MundialitoCorp.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Equipo> Equipos => Set<Equipo>();
        public DbSet<Jugador> Jugadores => Set<Jugador>();
        public DbSet<Partido> Partidos => Set<Partido>();
        public DbSet<IdempotencyKey> IdempotencyKeys => Set<IdempotencyKey>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}
