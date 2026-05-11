using MundialitoCorp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MundialitoCorp.Infrastructure.Persistence.Configurations
{
    public class EquipoConfiguration : IEntityTypeConfiguration<Equipo>
    {
        public void Configure(EntityTypeBuilder<Equipo> builder)
        {
            builder.ToTable("Equipos");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Id)
                   .ValueGeneratedNever();

            builder.Property(e => e.Nombre)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(e => e.PartidosJugados)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(e => e.PartidosGanados)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(e => e.PartidosPerdidos)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(e => e.PartidosEmpatados)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(e => e.Puntos)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(e => e.GolesFavor)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(e => e.GolesContra)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(e => e.DiferenciaGoles)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Ignore(e => e.DomainEvents);
        }
    }
}