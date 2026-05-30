using FlashSales.Application.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlashSales.Infrastructure.Outbox
{
    public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type).HasColumnType("VARCHAR(200)");
            builder.Property(x => x.Content).HasColumnType("JSONB");
            builder.Property(x => x.OccurredOn);
            builder.Property(x => x.RetryCount);
            builder.Property(x => x.NextRetryAt).IsRequired(false);
            builder.Property(x => x.IsPermanentFailure);
            builder.Property(x => x.CorrelationId);
            builder.Property(x => x.Error).IsRequired(false).HasColumnType("VARCHAR(256)");
            builder.Property(x => x.ProcessedOn).IsRequired(false);
            builder.HasIndex(x => x.CorrelationId)
                .IsUnique();
        }
    }
}