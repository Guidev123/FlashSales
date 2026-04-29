using MidR.Interfaces;
using Modules.Users.IntegrationEvents;

namespace Modules.Catalog.Infrastructure.IntegrationEvents
{
    internal sealed class SellerActivatedIntegrationEventHandler : INotificationHandler<SellerActivatedIntegrationEvent>
    {
        public Task ExecuteAsync(SellerActivatedIntegrationEvent notification, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}