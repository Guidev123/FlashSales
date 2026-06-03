using FlashSales.Application.Abstractions;
using FlashSales.Application.Bus;
using FlashSales.Application.Extensions;
using FlashSales.Application.Messaging;
using FlashSales.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace FlashSales.Infrastructure.Inbox
{
    public sealed class ModuleInboxConsumer<TUnitOfWork>(
        IEventBus eventBus,
        IServiceProvider serviceProvider,
        ILogger<ModuleInboxConsumer<TUnitOfWork>> logger,
        string moduleName,
        string subscriptionName,
        string[] topics
    ) : BackgroundService
        where TUnitOfWork : IUnitOfWork
    {
        private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(30);
        private readonly List<IAsyncDisposable> _subscriptions = [];

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SubscribeAsync(stoppingToken);
                    logger.LogInformation("[{Module}] Subscribed to integration event topics", moduleName);

                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "[{Module}] Failed to subscribe to Service Bus. Retrying in {Delay}s...",
                        moduleName,
                        RetryDelay.TotalSeconds);

                    await DisposeSubscriptionsAsync();
                    await Task.Delay(RetryDelay, stoppingToken);
                }
            }

            await DisposeSubscriptionsAsync();
            logger.LogInformation("[{Module}] Unsubscribed from integration event topics", moduleName);
        }

        private async Task SubscribeAsync(CancellationToken cancellationToken)
        {
            foreach (var topic in topics)
            {
                var subscription = await eventBus.SubscribeAsync(
                    topic,
                    subscriptionName,
                    HandleAsync,
                    cancellationToken: cancellationToken);

                _subscriptions.Add(subscription);
            }
        }

        private async Task HandleAsync(ConsumedMessage message, CancellationToken cancellationToken)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<TUnitOfWork>();
            var inboxRepository = scope.ServiceProvider.GetRequiredService<ModuleInboxRepository<TUnitOfWork>>();

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
                    "[{Module}] Failed to persist integration event {MessageType} [{MessageId}] to inbox",
                    moduleName,
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
