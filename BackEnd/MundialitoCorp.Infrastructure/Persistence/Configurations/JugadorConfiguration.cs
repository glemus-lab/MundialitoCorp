using MundialitoCorp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MundialitoCorp.Infrastructure.Persistence.Configurations
{
    public class JugadorConfiguration : IEntityTypeConfiguration<Jugador>
    {
        public void Configure(EntityTypeBuilder<Jugador> builder)
        {
            builder.ToTable("Jugadores");

            builder.HasKey(j => j.Id);

            builder.Property(j => j.Id)
                   .ValueGeneratedNever();

            builder.Property(j => j.Nombre)
                   .IsRequired()
                   .HasMaxLength(150);

            builder.Property(j => j.GolesAnotados)
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(j => j.EquipoId)
                   .IsRequired();

            builder.HasOne<Equipo>()
                   .WithMany(s => s.Jugadores)
                   .HasForeignKey(j => j.EquipoId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Ignore(j => j.DomainEvents);
        }
    }
}
