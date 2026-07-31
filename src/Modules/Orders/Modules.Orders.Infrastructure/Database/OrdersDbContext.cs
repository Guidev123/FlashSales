using FlashSales.Domain.DomainObjects;
using FlashSales.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Modules.Orders.Domain.Launches.Entities;
using Modules.Orders.Domain.Orders.Entities;
using Modules.Orders.Domain.Orders.Models;
using System.Reflection;

namespace Modules.Orders.Infrastructure.Database
{
    internal sealed class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
    {
        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<Launch> Launches { get; set; } = default!;
        public DbSet<OrderCreationSaga> OrderCreationSagas { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schemas.Orders);

            modelBuilder.Ignore<DomainEvent>();

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.AddOutboxAndInbox();

            base.OnModelCreating(modelBuilder);
        }
    }
}
