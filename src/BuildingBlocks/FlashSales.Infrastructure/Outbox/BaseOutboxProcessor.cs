using FlashSales.Application.Extensions;
using FlashSales.Application.Outbox;
using FlashSales.Application.Abstractions;
using FlashSales.Domain.DomainObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MidR.Interfaces;
using Newtonsoft.Json;

namespace FlashSales.Infrastructure.Outbox
{
    public abstract class BaseOutboxProcessor(
        ILogger logger,
        IOptions<OutboxOptions> options,
        IServiceProvider serviceProvider,
        string moduleName) : BackgroundService, IOutboxBatchProcessor
    {
        private readonly OutboxOptions _outboxOptions = options.Value;

        protected abstract IUnitOfWork GetUnitOfWork(IServiceProvider sp);
        protected abstract IOutboxRepository GetOutboxRepository(IServiceProvider sp);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(
                TimeSpan.FromSeconds(_outboxOptions.IntervalInSeconds));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await ProcessBatchAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[{Module}] Unhandled exception in outbox worker", moduleName);
                }
            }
        }

        public async Task ProcessBatchAsync(CancellationToken cancellationToken = default)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var unitOfWork = GetUnitOfWork(scope.ServiceProvider);
            var outboxRepository = GetOutboxRepository(scope.ServiceProvider);

            logger.LogInformation("[{Module}] Beginning to process outbox messages", moduleName);

            await unitOfWork.BeginTransactionAsync(cancellationToken);

            var outboxMessages = await outboxRepository.GetAsync(
                _outboxOptions.BatchSize,
                cancellationToken);

            foreach (var outboxMessage in outboxMessages)
            {
                await using var messageScope = scope.ServiceProvider.CreateAsyncScope();

                Exception? exception = null;

                try
                {
                    var domainEvent = JsonConvert.DeserializeObject<DomainEvent>(
                        outboxMessage.Content,
                        JsonSerializerSettingsExtensions.Instance)!;

                    var messagePublisher = messageScope.ServiceProvider.GetRequiredService<IPublisher>();
                    await messagePublisher.PublishAsync(domainEvent, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[{Module}] Exception while processing outbox message {MessageId}", moduleName, outboxMessage.Id);
                    exception = ex;
                }

                ApplyRetryPolicy(outboxMessage, exception);

                await outboxRepository.UpdateAsync(
                    exception,
                    outboxMessage,
                    cancellationToken);
            }

            await unitOfWork.CommitAsync(cancellationToken);

            logger.LogInformation("[{Module}] Completed process outbox messages", moduleName);
        }

        private void ApplyRetryPolicy(OutboxMessage outboxMessage, Exception? exception)
        {
            if (exception is null)
            {
                outboxMessage.ProcessedOn = DateTimeOffset.UtcNow;
                return;
            }

            if (exception is FlashSalesException)
            {
                outboxMessage.IsPermanentFailure = true;
                outboxMessage.Error = exception.Message.Length > 256 ? exception.Message[..256] : exception.Message;

                logger.LogWarning(
                    "[{Module}] Permanent failure on outbox message {MessageId}: {Error}",
                    moduleName,
                    outboxMessage.Id,
                    exception.Message);

                return;
            }

            outboxMessage.RetryCount++;
            outboxMessage.Error = exception.Message.Length > 256 ? exception.Message[..256] : exception.Message;

            if (outboxMessage.RetryCount >= _outboxOptions.MaxRetryCount)
            {
                outboxMessage.IsPermanentFailure = true;

                logger.LogError(
                    "[{Module}] Outbox message {MessageId} exceeded max retries ({MaxRetry}). Marked as permanent failure.",
                    moduleName,
                    outboxMessage.Id,
                    _outboxOptions.MaxRetryCount);

                return;
            }

            outboxMessage.NextRetryAt = DateTimeOffset.UtcNow.ComputeNextRetryAt(outboxMessage.RetryCount);

            logger.LogWarning(
                "[{Module}] Outbox message {MessageId} scheduled for retry {RetryCount}/{MaxRetry} at {NextRetryAt}",
                moduleName,
                outboxMessage.Id,
                outboxMessage.RetryCount,
                _outboxOptions.MaxRetryCount,
                outboxMessage.NextRetryAt);
        }
    }
}
