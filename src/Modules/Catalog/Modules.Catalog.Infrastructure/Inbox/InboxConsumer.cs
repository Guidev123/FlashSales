using FlashSales.Application.Bus;
using FlashSales.Application.Extensions;
using FlashSales.Application.Inbox;
using FlashSales.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Modules.Catalog.Application.Abstractions;
using Modules.Users.IntegrationEvents;
using Newtonsoft.Json;
using System.Text;

namespace Modules.Catalog.Infrastructure.Inbox
{
    internal sealed class InboxConsumer(
        IEventBus eventBus,
        IServiceProvider serviceProvider,
        ILogger<InboxConsumer> logger
        ) : IHostedService, IAsyncDisposable
    {
        private const string SubscriptionName = "catalog";

        private readonly List<IAsyncDisposable> _subscriptions = [];

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var sellerActivated = await eventBus.SubscribeAsync(
                Topics.SellerActivated,
                SubscriptionName,
                HandleAsync,
                cancellationToken: cancellationToken);

            _subscriptions.Add(sellerActivated);

            logger.LogInformation("[Catalog] Subscribed to integration event topics");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var subscription in _subscriptions)
                await subscription.DisposeAsync();

            _subscriptions.Clear();

            logger.LogInformation("[Catalog] Unsubscribed from integration event topics");
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var subscription in _subscriptions)
                await subscription.DisposeAsync();
        }

        private async Task HandleAsync(ConsumedMessage message, CancellationToken cancellationToken)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<ICatalogUnitOfWork>();
            var inboxRepository = scope.ServiceProvider.GetRequiredService<IInboxRepository>();

            try
            {
                var integrationEvent = JsonConvert.DeserializeObject<IntegrationEvent>(
                    Encoding.UTF8.GetString(message.Body.Span),
                    JsonSerializerSettingsExtensions.Instance)!;

                await unitOfWork.BeginTransactionAsync(cancellationToken);
                await inboxRepository.InsertAsync(integrationEvent, cancellationToken);
                await unitOfWork.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "[Catalog] Failed to persist integration event {MessageType} [{MessageId}] to inbox",
                    message.MessageType,
                    message.MessageId);

                throw;
            }
        }
    }
}