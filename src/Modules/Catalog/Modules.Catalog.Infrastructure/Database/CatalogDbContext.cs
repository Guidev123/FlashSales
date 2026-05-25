using FlashSales.Domain.DomainObjects;
using FlashSales.Infrastructure.Inbox;
using FlashSales.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Domain.Products.Entities;
using Modules.Catalog.Domain.Sellers.Entities;
using System.Reflection;

namespace Modules.Catalog.Infrastructure.Database
{
    internal sealed class CatalogDbContext(DbContextOptions<CatalogDbContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Seller> Sellers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schemas.Catalog);

            modelBuilder.Ignore<DomainEvent>();

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
            modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
            modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
            modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());

            base.OnModelCreating(modelBuilder);
        }
    }
}