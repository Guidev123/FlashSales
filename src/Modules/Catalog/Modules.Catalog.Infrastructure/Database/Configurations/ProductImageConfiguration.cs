using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Catalog.Domain.Products.Entities;

namespace Modules.Catalog.Infrastructure.Database.Configurations
{
    internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImages");

            builder.HasKey(pi => pi.Id);

            builder.Property(pi => pi.ProductId)
                .IsRequired();

            builder.Property(pi => pi.Url)
                .HasColumnType($"VARCHAR({ProductImage.URL_MAX_LENGTH})")
                .IsRequired();

            builder.Property(pi => pi.Order)
                .IsRequired();

            builder.Property(pi => pi.IsCover)
                .IsRequired();

            builder.Property(pi => pi.CreatedOn)
                .IsRequired();

            builder.HasOne<Product>()
                .WithMany()
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(pi => new { pi.ProductId, pi.Order })
                .HasDatabaseName("IX_ProductImages_ProductId_Order");
        }
    }
}
