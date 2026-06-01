using FlashSales.Application.Extensions;
using FlashSales.Application.Inbox;
using FlashSales.Application.Abstractions;
using FlashSales.Application.Messaging;
using FlashSales.Domain.DomainObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MidR.Interfaces;
using Newtonsoft.Json;

namespace FlashSales.Infrastructure.Inbox
{
    public abstract class BaseInboxProcessor(
        ILogger logger,
        IOptions<InboxOptions> options,
        IServiceProvider serviceProvider,
        string moduleName) : BackgroundService, IInboxBatchProcessor
    {
        private readonly InboxOptions _inboxOptions = options.Value;

        protected abstract IUnitOfWork GetUnitOfWork(IServiceProvider sp);
        protected abstract IInboxRepository GetInboxRepository(IServiceProvider sp);

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
                    logger.LogError(ex, "[{Module}] Unhandled exception in inbox worker", moduleName);
                }
            }
        }

        public async Task ProcessBatchAsync(CancellationToken cancellationToken = default)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var unitOfWork = GetUnitOfWork(scope.ServiceProvider);
            var inboxRepository = GetInboxRepository(scope.ServiceProvider);

            logger.LogInformation("[{Module}] Beginning to process inbox messages", moduleName);

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
                    logger.LogError(ex, "[{Module}] Exception while processing inbox message {MessageId}", moduleName, inboxMessage.Id);
                    exception = ex;
                }

                ApplyRetryPolicy(inboxMessage, exception);

                await inboxRepository.UpdateAsync(
                    exception,
                    inboxMessage,
                    cancellationToken);
            }

            await unitOfWork.CommitAsync(cancellationToken);

            logger.LogInformation("[{Module}] Completed process inbox messages", moduleName);
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
                    "[{Module}] Permanent failure on inbox message {MessageId}: {Error}",
                    moduleName,
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
                    "[{Module}] Inbox message {MessageId} exceeded max retries ({MaxRetry}). Marked as permanent failure.",
                    moduleName,
                    inboxMessage.Id,
                    _inboxOptions.MaxRetryCount);

                return;
            }

            inboxMessage.NextRetryAt = DateTimeOffset.UtcNow.ComputeNextRetryAt(inboxMessage.RetryCount);

            logger.LogWarning(
                "[{Module}] Inbox message {MessageId} scheduled for retry {RetryCount}/{MaxRetry} at {NextRetryAt}",
                moduleName,
                inboxMessage.Id,
                inboxMessage.RetryCount,
                _inboxOptions.MaxRetryCount,
                inboxMessage.NextRetryAt);
        }
    }
}
