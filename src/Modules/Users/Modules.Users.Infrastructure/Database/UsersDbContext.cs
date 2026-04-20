using FlashSales.Domain.DomainObjects;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain.Users.Entities;
using System.Reflection;

namespace Modules.Users.Infrastructure.Database
{
    internal sealed class UsersDbContext(DbContextOptions<UsersDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<SellerProfile> SellerProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schemas.Users);

            modelBuilder.Ignore<DomainEvent>();

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(modelBuilder);
        }
    }
}