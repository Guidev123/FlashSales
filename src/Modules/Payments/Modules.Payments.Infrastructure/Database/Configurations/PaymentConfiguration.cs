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

            builder.Property(p => p.Amount)
                .IsRequired();

            builder.Property(p => p.ExpiresAt)
                .IsRequired();

            builder.Property(p => p.Status)
                .HasColumnType("VARCHAR(50)")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(p => p.CreatedOn)
                .IsRequired();

            builder.HasIndex(p => p.OrderId)
                .IsUnique();

            builder.HasMany(p => p.Attempts)
                .WithOne()
                .HasForeignKey(a => a.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(p => p.Attempts).HasField("_attempts");
        }
    }
}
