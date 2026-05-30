using FlashSales.Application.Bus;
using FlashSales.Application.Extensions;
using FlashSales.Application.Inbox;
using FlashSales.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Modules.Users.Application.Abstractions;
using Newtonsoft.Json;
using System.Text;

namespace Modules.Users.Infrastructure.Inbox
{
#pragma warning disable CS9113 // eventBus will be used when topics are subscribed

    internal sealed class InboxConsumer(
        IEventBus eventBus,
        IServiceProvider serviceProvider,
        ILogger<InboxConsumer> logger
        ) : BackgroundService
#pragma warning restore CS9113
    {
        private const string SubscriptionName = "users.sub";
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(30);

        private readonly List<IAsyncDisposable> _subscriptions = [];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SubscribeAsync(stoppingToken);
                    logger.LogInformation("[Users] Subscribed to integration event topics");

                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "[Users] Failed to subscribe to Service Bus. Retrying in {Delay}s...",
                        RetryDelay.TotalSeconds);

                    await DisposeSubscriptionsAsync();
                    await Task.Delay(RetryDelay, stoppingToken);
                }
            }

            await DisposeSubscriptionsAsync();
            logger.LogInformation("[Users] Unsubscribed from integration event topics");
        }

        private Task SubscribeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private async Task HandleAsync(ConsumedMessage message, CancellationToken cancellationToken)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUsersUnitOfWork>();
            var inboxRepository = scope.ServiceProvider.GetRequiredService<IUsersInboxRepository>();

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
                    "[Users] Failed to persist integration event {MessageType} [{MessageId}] to inbox",
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