using FlashSales.Domain.DomainObjects;
using FlashSales.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Modules.Launches.Domain.Launches.Entities;
using Modules.Launches.Domain.Sellers.Entities;
using System.Reflection;

namespace Modules.Launches.Infrastructure.Database
{
    internal sealed class LaunchesDbContext(DbContextOptions<LaunchesDbContext> options) : DbContext(options)
    {
        public DbSet<Launch> Launches { get; set; } = default!;
        public DbSet<Seller> Sellers { get; set; } = default!;
        public DbSet<StockReservation> StockReservations { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schemas.Launches);

            modelBuilder.Ignore<DomainEvent>();

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.AddOutboxAndInbox();

            base.OnModelCreating(modelBuilder);
        }
    }
}