using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.AccessManagement.Models;
using Modules.Users.Domain.Users.Entities;

namespace Modules.Users.Infrastructure.Database.Configurations
{
    internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");

            builder.HasKey(r => r.Name);

            builder.Property(r => r.Name)
                .HasColumnType("VARCHAR(50)")
                .IsRequired();

            builder
                .HasMany<User>()
                .WithMany(u => u.Roles)
                .UsingEntity(joinBuilder =>
                {
                    joinBuilder.ToTable("UserRoles");

                    joinBuilder.Property("RolesName").HasColumnName("RoleName");
                });
        }
    }
}