using FlashSales.Application.Extensions;
using FlashSales.Application.Inbox;
using FlashSales.Application.Messaging;
using FlashSales.Domain.DomainObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MidR.Interfaces;
using Modules.Catalog.Application.Abstractions;
using Newtonsoft.Json;

namespace Modules.Catalog.Infrastructure.Inbox
{
    internal sealed class InboxProcessor(
        ILogger<InboxProcessor> logger,
        IOptions<InboxOptions> options,
        IServiceProvider serviceProvider
        ) : BackgroundService, IInboxBatchProcessor
    {
        private readonly InboxOptions _inboxOptions = options.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(
                TimeSpan.FromSeconds(_inboxOptions.IntervalInSeconds));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await ProcessBatchAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[Catalog] Unhandled exception in inbox worker");
                }
            }
        }

        public async Task ProcessBatchAsync(CancellationToken cancellationToken = default)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<ICatalogUnitOfWork>();
            var inboxRepository = scope.ServiceProvider.GetRequiredService<ICatalogInboxRepository>();

            logger.LogInformation("[Catalog] Beginning to process inbox messages");

            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var inboxMessages = await inboxRepository.GetAsync(
                _inboxOptions.BatchSize,
                cancellationToken);

            foreach (var inboxMessage in inboxMessages)
            {
                await using var messageScope = scope.ServiceProvider.CreateAsyncScope();

                Exception? exception = null;

                try
                {
                    var integrationEvent = JsonConvert.DeserializeObject<IntegrationEvent>(
                        inboxMessage.Content,
                        JsonSerializerSettingsExtensions.Instance)!;

                    var messagePublisher = messageScope.ServiceProvider.GetRequiredService<IPublisher>();
                    await messagePublisher.PublishAsync(integrationEvent, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[Catalog] Exception while processing inbox message {MessageId}", inboxMessage.Id);
                    exception = ex;
                }

                ApplyRetryPolicy(inboxMessage, exception);

                await inboxRepository.UpdateAsync(
                    exception,
                    inboxMessage,
                    cancellationToken);
            }

            await unitOfWork.CommitAsync(cancellationToken);

            logger.LogInformation("[Catalog] Completed process inbox messages");
        }

        private void ApplyRetryPolicy(InboxMessage inboxMessage, Exception? exception)
        {
            if (exception is null)
            {
                inboxMessage.ProcessedOn = DateTimeOffset.UtcNow;
                return;
            }

            if (exception is FlashSalesException)
            {
                inboxMessage.IsPermanentFailure = true;
                inboxMessage.Error = exception.Message.Length > 256 ? exception.Message[..256] : exception.Message;

                logger.LogWarning(
                    "[Catalog] Permanent failure on inbox message {MessageId}: {Error}",
                    inboxMessage.Id,
                    exception.Message);

                return;
            }

            inboxMessage.RetryCount++;
            inboxMessage.Error = exception.Message.Length > 256 ? exception.Message[..256] : exception.Message;

            if (inboxMessage.RetryCount >= _inboxOptions.MaxRetryCount)
            {
                inboxMessage.IsPermanentFailure = true;

                logger.LogError(
                    "[Catalog] Inbox message {MessageId} exceeded max retries ({MaxRetry}). Marked as permanent failure.",
                    inboxMessage.Id,
                    _inboxOptions.MaxRetryCount);

                return;
            }

            inboxMessage.NextRetryAt = DateTimeOffset.UtcNow.ComputeNextRetryAt(inboxMessage.RetryCount);

            logger.LogWarning(
                "[Catalog] Inbox message {MessageId} scheduled for retry {RetryCount}/{MaxRetry} at {NextRetryAt}",
                inboxMessage.Id,
                inboxMessage.RetryCount,
                _inboxOptions.MaxRetryCount,
                inboxMessage.NextRetryAt);
        }
    }
}
