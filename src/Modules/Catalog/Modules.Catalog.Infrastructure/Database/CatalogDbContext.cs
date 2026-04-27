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
        public DbSet<ProductImage> ProductsImage { get; set; }
        public DbSet<Seller> Sellers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            modelBuilder.HasDefaultSchema(Schemas.Catalog);

            modelBuilder.Ignore<DomainEvent>();

            base.OnModelCreating(modelBuilder);
        }
    }
}