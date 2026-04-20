using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.Users.Entities;
using Modules.Users.Domain.Users.ValueObjects;

namespace Modules.Users.Infrastructure.Database.Configurations
{
    internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.OwnsOne(c => c.Email, email =>
            {
                email.Property(c => c.Address)
                    .HasColumnType("VARCHAR(160)")
                    .HasColumnName(nameof(Email))
                    .IsRequired();

                email.HasIndex(e => e.Address)
                    .IsUnique()
                    .HasDatabaseName("IX_Users_Email");
            });

            builder.OwnsOne(c => c.Name, name =>
            {
                name.Property(c => c.FirstName)
                    .HasColumnType("VARCHAR(100)")
                    .HasColumnName("FirstName")
                    .IsRequired();

                name.Property(c => c.LastName)
                    .HasColumnType("VARCHAR(100)")
                    .HasColumnName("LastName")
                    .IsRequired();
            });

            builder.OwnsOne(c => c.Age, age =>
            {
                age.Property(c => c.BirthDate)
                    .HasColumnName("BirthDate")
                    .IsRequired();
            });

            builder.Property(c => c.CreatedOn)
                .IsRequired();

            builder.Property(c => c.IdentiyProviderId)
                .HasColumnType("VARCHAR(50)")
                .IsRequired();

            builder.Property(c => c.IsDeleted)
                .IsRequired();

            builder.Property(c => c.DeletedOn)
                .IsRequired(false);

            builder.HasIndex(c => new { c.CreatedOn, c.Id });
        }
    }
}