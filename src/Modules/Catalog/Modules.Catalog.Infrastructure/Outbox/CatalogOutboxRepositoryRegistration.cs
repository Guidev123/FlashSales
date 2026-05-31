using FlashSales.Application.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Modules.Catalog.Infrastructure.Database.Repositories;

namespace Modules.Catalog.Infrastructure.Outbox
{
    internal sealed class CatalogOutboxRepositoryRegistration : IOutboxRepositoryRegistration
    {
        public bool Matches(Type commandType)
            => CatalogModule.Assemblies.Contains(commandType.Assembly);

        public IOutboxRepository Resolve(IServiceProvider sp)
            => sp.GetRequiredService<OutboxRepository>();
    }
}