using FlashSales.Application.Bus;
using FlashSales.Application.Extensions;
using FlashSales.Application.Inbox;
using FlashSales.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using FlashSales.Infrastructure.Database;
using Modules.Catalog.Application.Abstractions;
using Modules.Users.Contracts.IntegrationEvents;
using Newtonsoft.Json;
using System.Text;

namespace Modules.Catalog.Infrastructure.Inbox
{
    internal sealed class InboxConsumer(
        IEventBus eventBus,
        IServiceProvider serviceProvider,
        ILogger<InboxConsumer> logger
        ) : BackgroundService
    {
        private const string SubscriptionName = "catalog.sub";
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(30);

        private readonly List<IAsyncDisposable> _subscriptions = [];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SubscribeAsync(stoppingToken);
                    logger.LogInformation("[Catalog] Subscribed to integration event topics");

                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "[Catalog] Failed to subscribe to Service Bus. Retrying in {Delay}s...",
                        RetryDelay.TotalSeconds);

                    await DisposeSubscriptionsAsync();
                    await Task.Delay(RetryDelay, stoppingToken);
                }
            }

            await DisposeSubscriptionsAsync();
            logger.LogInformation("[Catalog] Unsubscribed from integration event topics");
        }

        private async Task SubscribeAsync(CancellationToken cancellationToken)
        {
            var sellerActivated = await eventBus.SubscribeAsync(
                Topics.SellerActivated,
                SubscriptionName,
                HandleAsync,
                cancellationToken: cancellationToken);

            var sellerProfilePictureUpdated = await eventBus.SubscribeAsync(
                Topics.SellerProfilePictureUpdated,
                SubscriptionName,
                HandleAsync,
                cancellationToken: cancellationToken);

            var userProfileUpdated = await eventBus.SubscribeAsync(
                Topics.UserProfileUpdated,
                SubscriptionName,
                HandleAsync,
                cancellationToken: cancellationToken);

            _subscriptions.Add(sellerActivated);
            _subscriptions.Add(sellerProfilePictureUpdated);
            _subscriptions.Add(userProfileUpdated);
        }

        private async Task HandleAsync(ConsumedMessage message, CancellationToken cancellationToken)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<ICatalogUnitOfWork>();
            var inboxRepository = scope.ServiceProvider.GetRequiredService<ModuleInboxRepository<ICatalogUnitOfWork>>();

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

        private async Task DisposeSubscriptionsAsync()
        {
            foreach (var subscription in _subscriptions)
                await subscription.DisposeAsync();

            _subscriptions.Clear();
        }
    }
}