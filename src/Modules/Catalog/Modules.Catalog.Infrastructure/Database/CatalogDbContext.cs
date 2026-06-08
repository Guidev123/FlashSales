using FlashSales.Domain.DomainObjects;
using FlashSales.Infrastructure.Extensions;
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
            modelBuilder.AddOutboxAndInbox();

            base.OnModelCreating(modelBuilder);
        }
    }
}