using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Orders.Domain.Orders.Entities;

namespace Modules.Orders.Infrastructure.Database.Configurations
{
    internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.CustomerId).IsRequired();
            builder.Property(o => o.LaunchId).IsRequired();
            builder.Property(o => o.SellerId).IsRequired();
            builder.Property(o => o.ProductId).IsRequired();
            builder.Property(o => o.Quantity).IsRequired();
            builder.Property(o => o.UnitPrice).IsRequired();
            builder.Property(o => o.TotalAmount).IsRequired();

            builder.Property(o => o.OrderCode)
                .HasColumnType("VARCHAR(20)")
                .IsRequired();

            builder.Property(o => o.Status)
                .HasColumnType("VARCHAR(50)")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(o => o.ExpiresAt).IsRequired();
            builder.Property(o => o.Reason).HasColumnType("VARCHAR(500)");
            builder.Property(o => o.CreatedOn).IsRequired();

            builder.HasIndex(o => o.OrderCode).IsUnique();

            builder.HasIndex(o => new { o.CustomerId, o.LaunchId })
                .HasFilter("\"Status\" IN ('AwaitingPayment', 'PaymentProcessing')")
                .IsUnique();

            builder.HasIndex(o => new { o.CustomerId, o.LaunchId, o.Status });

            builder.HasIndex(o => new { o.Status, o.ExpiresAt });
        }
    }
}
