using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.Users.Models;

namespace Modules.Users.Infrastructure.Database.Configurations
{
    internal sealed class RegistrationTypeRolesConfiguration : IEntityTypeConfiguration<RegistrationTypeRoles>
    {
        public void Configure(EntityTypeBuilder<RegistrationTypeRoles> builder)
        {
            builder.ToTable("RegistrationTypeRoles");

            builder.HasKey(x => new { x.Type, x.RoleName });

            builder.Property(x => x.Type)
                .HasConversion<string>()
                .HasColumnType("VARCHAR(30)")
                .IsRequired();

            builder.Property(x => x.RoleName)
                .HasColumnType("VARCHAR(50)")
                .IsRequired();

            builder
                .HasOne<Role>()
                .WithMany()
                .HasForeignKey(x => x.RoleName)
                .OnDelete(DeleteBehavior.ClientNoAction);
        }
    }
}