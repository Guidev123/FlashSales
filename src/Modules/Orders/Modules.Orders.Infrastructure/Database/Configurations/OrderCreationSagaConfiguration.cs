using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Orders.Domain.Orders.Models;

namespace Modules.Orders.Infrastructure.Database.Configurations
{
    internal sealed class OrderCreationSagaConfiguration : IEntityTypeConfiguration<OrderCreationSaga>
    {
        public void Configure(EntityTypeBuilder<OrderCreationSaga> builder)
        {
            builder.ToTable("OrderCreationSagas");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.Step)
                .HasColumnType("VARCHAR(50)")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(s => s.RetryCount).IsRequired();
            builder.Property(s => s.LastError).HasColumnType("VARCHAR(500)");
            builder.Property(s => s.CreatedOn).IsRequired();

            builder.HasIndex(s => new { s.Step, s.CreatedOn });
        }
    }
}
