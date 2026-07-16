using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Payments.Domain.Payments.Entities;

namespace Modules.Payments.Infrastructure.Database.Configurations
{
    internal sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.OrderId)
                .IsRequired();

            builder.Property(p => p.CustomerId)
                .IsRequired();

            builder.Property(p => p.TransactionId)
                .IsRequired();

            builder.Property(p => p.Amount)
                .IsRequired();

            builder.Property(p => p.OrderCode)
                .HasColumnType($"VARCHAR({Payment.ORDER_CODE_MAX_LENGTH})")
                .IsRequired();

            builder.Property(p => p.ExternalReference)
                .HasColumnType("VARCHAR(100)");

            builder.Property(p => p.Status)
                .HasColumnType("VARCHAR(50)")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(p => p.CreatedOn)
                .IsRequired();

            builder.HasIndex(p => p.TransactionId)
                .IsUnique();

            builder.HasIndex(p => p.OrderId);

            builder.HasIndex(p => new { p.CustomerId, p.CreatedOn });
        }
    }
}