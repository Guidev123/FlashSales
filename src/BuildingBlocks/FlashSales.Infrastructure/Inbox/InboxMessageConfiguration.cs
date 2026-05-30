using FlashSales.Application.Inbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlashSales.Infrastructure.Inbox
{
    public sealed class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
    {
        public void Configure(EntityTypeBuilder<InboxMessage> builder)
        {
            builder.ToTable("InboxMessages");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type).HasColumnType("VARCHAR(200)");
            builder.Property(x => x.Content).HasColumnType("NVARCHAR(MAX)");
            builder.Property(x => x.OccurredOn).HasColumnType("DATETIME2");
            builder.Property(x => x.RetryCount).HasColumnType("INT");
            builder.Property(x => x.NextRetryAt).IsRequired(false).HasColumnType("DATETIME2");
            builder.Property(x => x.IsPermanentFailure).HasColumnType("BIT");
            builder.Property(x => x.CorrelationId).HasColumnType("UNIQUEIDENTIFIER");
            builder.Property(x => x.Error).IsRequired(false).HasColumnType("VARCHAR(256)");
            builder.Property(x => x.ProcessedOn).IsRequired(false).HasColumnType("DATETIME2");
            builder.HasIndex(x => x.CorrelationId)
                .IsUnique();
        }
    }
}
