using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Catalog.Domain.Sellers.Entities;

namespace Modules.Catalog.Infrastructure.Database.Configurations
{
    internal sealed class SellerConfiguration : IEntityTypeConfiguration<Seller>
    {
        public void Configure(EntityTypeBuilder<Seller> builder)
        {
            builder.ToTable("Sellers");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.UserId)
                .IsRequired();

            builder.Property(s => s.SellerId)
                .IsRequired();

            builder.Property(s => s.Name)
                .HasColumnType($"VARCHAR({Seller.NAME_MAX_LENGTH})")
                .IsRequired();

            builder.Property(s => s.ProfilePictureUrl)
                .HasColumnType($"VARCHAR({Seller.PROFILE_PICTURE_URL_MAX_LENGTH})")
                .IsRequired(false);

            builder.Property(s => s.IsActive)
                .IsRequired();

            builder.Property(s => s.CreatedOn)
                .IsRequired();

            builder.HasIndex(s => s.SellerId)
                .IsUnique()
                .HasDatabaseName("IX_Sellers_SellerId");

            builder.HasIndex(s => s.UserId)
                .IsUnique()
                .HasDatabaseName("IX_Sellers_UserId");
        }
    }
}
