using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Catalog.Domain.Products.Entities;
using Modules.Catalog.Domain.Products.ValueObjects;
using Modules.Catalog.Domain.Sellers.Entities;

namespace Modules.Catalog.Infrastructure.Database.Configurations
{
    internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.SellerId)
                .IsRequired();

            builder.Property(p => p.CategoryId)
                .IsRequired();

            builder.Property(p => p.Status)
                .HasColumnType("VARCHAR(50)")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(p => p.CreatedOn)
                .IsRequired();

            builder.OwnsOne(p => p.Metadata, metadata =>
            {
                metadata.Property(m => m.Name)
                    .HasColumnName("Name")
                    .HasColumnType($"VARCHAR({ProductMetadata.NAME_MAX_LENGTH})")
                    .IsRequired();

                metadata.Property(m => m.Description)
                    .HasColumnName("Description")
                    .HasColumnType("TEXT")
                    .IsRequired();
            });

            builder.HasOne<Category>()
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Seller>()
                .WithMany()
                .HasForeignKey(p => p.SellerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(p => p.Images)
                .WithOne()
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => new { p.SellerId, p.CreatedOn });
            builder.HasIndex(p => new { p.CategoryId, p.CreatedOn });
        }
    }
}