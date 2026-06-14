using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Launches.Domain.Launches.Entities;

namespace Modules.Launches.Infrastructure.Database.Configurations
{
    internal sealed class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
    {
        public void Configure(EntityTypeBuilder<StockReservation> builder)
        {
            builder.ToTable("StockReservations");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.LaunchId).IsRequired();
            builder.Property(r => r.OrderId).IsRequired();
            builder.Property(r => r.Quantity).IsRequired();
            builder.Property(r => r.CreatedOn).IsRequired();

            builder.HasIndex(r => new { r.LaunchId, r.OrderId }).IsUnique();
        }
    }
}
