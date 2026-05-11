using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MundialitoCorp.Domain.Entities;

namespace MundialitoCorp.Infrastructure.Persistence.Configurations
{
    public class PartidoGolConfiguration : IEntityTypeConfiguration<PartidoGol>
    {
        public void Configure(EntityTypeBuilder<PartidoGol> builder)
        {
            builder.ToTable("PartidoGoles");
            builder.HasKey(g => g.Id);

            builder.HasOne<Partido>()
               .WithMany(p => p.GolesDetalle)
               .HasForeignKey(g => g.PartidoId)
               .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Jugador>()
               .WithMany()
               .HasForeignKey(g => g.JugadorId)
               .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
