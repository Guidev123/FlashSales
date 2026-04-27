using FlashSales.Domain.DomainObjects;
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

            base.OnModelCreating(modelBuilder);
        }
    }
}