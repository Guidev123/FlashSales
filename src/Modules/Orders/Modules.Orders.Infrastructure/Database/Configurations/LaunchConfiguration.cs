using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Orders.Domain.Launches.Entities;

namespace Modules.Orders.Infrastructure.Database.Configurations
{
    internal sealed class LaunchConfiguration : IEntityTypeConfiguration<Launch>
    {
        public void Configure(EntityTypeBuilder<Launch> builder)
        {
            builder.ToTable("Launches");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.SellerId).IsRequired();
            builder.Property(l => l.ProductId).IsRequired();

            builder.Property(l => l.Title)
                .HasColumnType("VARCHAR(200)")
                .IsRequired();

            builder.Property(l => l.DiscountedPrice).IsRequired();
            builder.Property(l => l.OriginalPrice).IsRequired();
            builder.Property(l => l.TotalQuantity).IsRequired();
            builder.Property(l => l.StartAt).IsRequired();
            builder.Property(l => l.EndAt).IsRequired();

            builder.Property(l => l.Status)
                .HasColumnType("VARCHAR(50)")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(l => l.CreatedOn).IsRequired();
        }
    }
}
