using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Launches.Domain.Launches.Entities;
using Modules.Launches.Domain.Launches.Enums;
using Modules.Launches.Domain.Launches.ValueObjects;

namespace Modules.Launches.Infrastructure.Database.Configurations
{
    internal sealed class LaunchConfiguration : IEntityTypeConfiguration<Launch>
    {
        public void Configure(EntityTypeBuilder<Launch> builder)
        {
            builder.ToTable("Launches");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.SellerId)
                .IsRequired();

            builder.Property(l => l.ProductId)
                .IsRequired();

            builder.Property(l => l.Status)
                .HasColumnType("VARCHAR(50)")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(l => l.CreatedOn)
                .IsRequired();

            builder.Property<uint>("Version")
                .HasColumnName("xmin")
                .HasColumnType("xid")
                .IsRowVersion();

            builder.OwnsOne(l => l.Metadata, metadata =>
            {
                metadata.Property(m => m.Title)
                    .HasColumnName("Title")
                    .HasColumnType($"VARCHAR({LaunchMetadata.TITLE_MAX_LENGTH})")
                    .IsRequired();

                metadata.Property(m => m.Description)
                    .HasColumnName("Description")
                    .HasColumnType($"VARCHAR({LaunchMetadata.DESCRIPTION_MAX_LENGTH})")
                    .IsRequired();
            });

            builder.OwnsOne(l => l.Price, price =>
            {
                price.Property(p => p.DiscountedPrice)
                    .HasColumnName("DiscountedPrice")
                    .HasColumnType("numeric(18,2)")
                    .IsRequired();

                price.Property(p => p.OriginalPrice)
                    .HasColumnName("OriginalPrice")
                    .HasColumnType("numeric(18,2)")
                    .IsRequired();
            });

            builder.OwnsOne(l => l.Stock, stock =>
            {
                stock.Property(s => s.TotalQuantity)
                    .HasColumnName("TotalQuantity")
                    .IsRequired();

                stock.Property(s => s.ReservedQuantity)
                    .HasColumnName("ReservedQuantity")
                    .HasDefaultValue(0)
                    .ValueGeneratedNever()
                    .IsRequired();

                stock.Ignore(s => s.AvailableQuantity);
            });

            builder.OwnsOne(l => l.Schedule, schedule =>
            {
                schedule.Property(s => s.StartAt)
                    .HasColumnName("StartAt")
                    .IsRequired();

                schedule.Property(s => s.EndAt)
                    .HasColumnName("EndAt")
                    .IsRequired();
            });

            builder.HasMany(l => l.StockReservations)
                .WithOne()
                .HasForeignKey(r => r.LaunchId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(l => l.StockReservations).HasField("_stockReservations");

            builder.HasIndex(l => new { l.SellerId, l.Status });
        }
    }
}