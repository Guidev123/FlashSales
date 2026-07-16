using FlashSales.Domain.DomainObjects;
using FlashSales.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Modules.Payments.Domain.Payments.Entities;
using System.Reflection;

namespace Modules.Payments.Infrastructure.Database
{
    internal sealed class PaymentsDbContext(DbContextOptions<PaymentsDbContext> options) : DbContext(options)
    {
        public DbSet<Payment> Payments { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(Schemas.Payments);

            modelBuilder.Ignore<DomainEvent>();

            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            modelBuilder.AddOutboxAndInbox();

            base.OnModelCreating(modelBuilder);
        }
    }
}