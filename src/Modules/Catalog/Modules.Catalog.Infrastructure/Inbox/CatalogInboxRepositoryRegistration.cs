using FlashSales.Application.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Modules.Catalog.Infrastructure.Database.Repositories;
using Modules.Users.IntegrationEvents;

namespace Modules.Catalog.Infrastructure.Inbox
{
    internal sealed class CatalogInboxRepositoryRegistration : IInboxRepositoryRegistration
    {
        public bool Matches(Type commandType)
            => commandType.Assembly == typeof(SellerActivatedIntegrationEvent).Assembly;

        public IInboxRepository Resolve(IServiceProvider sp)
            => sp.GetRequiredService<InboxRepository>();
    }
}
