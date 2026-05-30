using FlashSales.Application.Abstractions;
using FlashSales.Application.Extensions;
using FlashSales.Application.Outbox;
using FlashSales.Domain.DomainObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MidR.Interfaces;
using Newtonsoft.Json;

namespace Modules.Users.Infrastructure.Outbox
{
    internal sealed class OutboxProcessor(
        ILogger<OutboxProcessor> logger,
        IOptions<OutboxOptions> options,
        IServiceProvider serviceProvider
        ) : BackgroundService
    {
        private readonly OutboxOptions _outboxOptions = options.Value;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(
                TimeSpan.FromSeconds(_outboxOptions.IntervalInSeconds));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                try
                {
                    await ProcessOutboxAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[Users] Unhandled exception in outbox worker");
                }
            }
        }

        private async Task ProcessOutboxAsync(CancellationToken stoppingToken)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var outboxRepository = scope.ServiceProvider.GetRequiredService<IOutboxRepository>();
            var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();

            logger.LogInformation("[Users] Beginning to process outbox messages");

            await unitOfWork.BeginTransactionAsync(stoppingToken);

            var outboxMessages = await outboxRepository.GetAsync(
                _outboxOptions.BatchSize,
                stoppingToken);

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
                    await messagePublisher.PublishAsync(domainEvent, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[Users] Exception while processing outbox message {MessageId}", outboxMessage.Id);
                    exception = ex;
                }

                ApplyRetryPolicy(outboxMessage, exception);

                await outboxRepository.UpdateAsync(
                    exception,
                    outboxMessage,
                    stoppingToken);
            }

            await unitOfWork.CommitAsync(stoppingToken);

            logger.LogInformation("[Users] Completed process outbox messages");
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
                outboxMessage.Error = exception.Message;

                logger.LogWarning(
                    "[Users] Permanent failure on outbox message {MessageId}: {Error}",
                    outboxMessage.Id,
                    exception.Message);

                return;
            }

            outboxMessage.RetryCount++;
            outboxMessage.Error = exception.Message;

            if (outboxMessage.RetryCount >= _outboxOptions.MaxRetryCount)
            {
                outboxMessage.IsPermanentFailure = true;

                logger.LogError(
                    "[Users] Outbox message {MessageId} exceeded max retries ({MaxRetry}). Marked as permanent failure.",
                    outboxMessage.Id,
                    _outboxOptions.MaxRetryCount);

                return;
            }

            outboxMessage.NextRetryAt = DateTimeOffset.UtcNow.ComputeNextRetryAt(outboxMessage.RetryCount);

            logger.LogWarning(
                "[Users] Outbox message {MessageId} scheduled for retry {RetryCount}/{MaxRetry} at {NextRetryAt}",
                outboxMessage.Id,
                outboxMessage.RetryCount,
                _outboxOptions.MaxRetryCount,
                outboxMessage.NextRetryAt);
        }

    }
}