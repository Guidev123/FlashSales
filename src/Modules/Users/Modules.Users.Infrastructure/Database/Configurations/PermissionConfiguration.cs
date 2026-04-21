using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.AccessManagement.Models;

namespace Modules.Users.Infrastructure.Database.Configurations
{
    internal sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");

            builder.HasKey(p => p.Code);

            builder.Property(p => p.Code)
                .HasColumnType("VARCHAR(100)")
                .IsRequired();

            builder
                .HasMany<Role>()
                .WithMany()
                .UsingEntity(joinBuilder =>
                {
                    joinBuilder.ToTable("RolePermissions");
                });
        }
    }
}