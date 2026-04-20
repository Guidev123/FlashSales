using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.Users.Entities;

namespace Modules.Users.Domain.Users.Models
{
    internal sealed class SellerProfileConfiguration : IEntityTypeConfiguration<SellerProfile>
    {
        public void Configure(EntityTypeBuilder<SellerProfile> builder)
        {
            builder.ToTable("SellerProfiles");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.UserId)
                .IsRequired();

            builder.Property(c => c.Status)
                .HasColumnType("VARCHAR(50)")
                .HasConversion<string>()
                .IsRequired();

            builder.Property(c => c.ActivatedOn)
                .IsRequired(false);

            builder.Property(c => c.CreatedOn)
                .IsRequired();

            builder.OwnsOne(c => c.Document, document =>
            {
                document.Property(d => d.Number)
                    .HasColumnType("VARCHAR(50)")
                    .HasColumnName("Document")
                    .IsRequired();

                document.Property(d => d.Type)
                    .HasColumnType("VARCHAR(50)")
                    .HasColumnName("DocumentType")
                    .HasConversion<string>()
                    .IsRequired();
            });

            builder.OwnsOne(c => c.PaymentAccount, paymentAccount =>
            {
                paymentAccount.Property(p => p.BankCode)
                    .HasColumnType("VARCHAR(50)")
                    .HasColumnName("BankCode")
                    .IsRequired();

                paymentAccount.Property(p => p.Agency)
                    .HasColumnType("VARCHAR(50)")
                    .HasColumnName("Agency")
                    .IsRequired();

                paymentAccount.Property(p => p.Number)
                    .HasColumnType("VARCHAR(50)")
                    .HasColumnName("AccountNumber")
                    .IsRequired();

                paymentAccount.Property(p => p.Type)
                    .HasColumnType("VARCHAR(50)")
                    .HasColumnName("AccountType")
                    .HasConversion<string>()
                    .IsRequired();
            });

            builder.HasOne<User>()
                .WithOne()
                .HasForeignKey<SellerProfile>(sp => sp.UserId)
                .IsRequired();
        }
    }
}