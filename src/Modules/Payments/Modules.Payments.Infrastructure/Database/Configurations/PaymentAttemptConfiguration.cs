using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Payments.Domain.Payments.Entities;

namespace Modules.Payments.Infrastructure.Database.Configurations
{
    internal sealed class PaymentAttemptConfiguration : IEntityTypeConfiguration<PaymentAttempt>
    {
        public void Configure(EntityTypeBuilder<PaymentAttempt> builder)
        {
            builder.ToTable("PaymentAttempts");

            builder.HasKey(a => a.Id);

            builder.Property(a => a.PaymentId).IsRequired();
            builder.Property(a => a.AttemptNumber).IsRequired();

            builder.Property(a => a.GatewaySessionId).HasColumnType("VARCHAR(255)");
            builder.Property(a => a.ExternalId).HasColumnType("VARCHAR(255)");
            builder.Property(a => a.GatewayResultCode).HasColumnType("VARCHAR(100)");
            builder.Property(a => a.GatewayResultMessage).HasColumnType("VARCHAR(500)");

            builder.Property(a => a.Status)
                .HasColumnType("VARCHAR(50)")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(a => a.CreatedOn).IsRequired();

            builder.HasIndex(a => new { a.PaymentId, a.AttemptNumber }).IsUnique();
        }
    }
}
