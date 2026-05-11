using MundialitoCorp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MundialitoCorp.Infrastructure.Persistence.Configurations
{
    public class PartidoConfiguration : IEntityTypeConfiguration<Partido>
    {
        public void Configure(EntityTypeBuilder<Partido> builder)
        {
            builder.ToTable("Partidos");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Id)
                   .ValueGeneratedNever();

            builder.Property(p => p.EquipoLocalId)
                   .IsRequired();

            builder.Property(p => p.EquipoVisitanteId)
                   .IsRequired();

            builder.Property(p => p.GolesLocal)
                   .IsRequired(false);

            builder.Property(p => p.GolesVisitante)
                   .IsRequired(false);

            builder.Property(p => p.Fecha)
                   .IsRequired();

            builder.Property(p => p.EstaFinalizado)
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.HasOne<Equipo>()
                   .WithMany()
                   .HasForeignKey(p => p.EquipoLocalId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Equipo>()
                   .WithMany()
                   .HasForeignKey(p => p.EquipoVisitanteId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Navigation(p => p.GolesDetalle)
                   .UsePropertyAccessMode(PropertyAccessMode.Field);

            builder.Ignore(p => p.DomainEvents);
        }
    }
}
