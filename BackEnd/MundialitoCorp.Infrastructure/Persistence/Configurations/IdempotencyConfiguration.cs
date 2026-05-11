using MundialitoCorp.Infrastructure.Persistence.Idempotency;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MundialitoCorp.Infrastructure.Persistence.Configurations
{
    public class IdempotencyConfiguration : IEntityTypeConfiguration<IdempotencyKey>
    {
        public void Configure(EntityTypeBuilder<IdempotencyKey> builder)
        {
            builder.ToTable("IdempotencyKeys");
            builder.HasKey(x => x.Key);
            builder.Property(x => x.Key).ValueGeneratedNever();
            builder.Property(x => x.RequestPath).IsRequired();
            builder.Property(x => x.ResponseBody).IsRequired();
        }
    }
}
