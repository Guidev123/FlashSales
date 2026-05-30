using FlashSales.Application.Extensions;
using FlashSales.Application.Inbox;
using FlashSales.Application.Messaging;
using FlashSales.Domain.DomainObjects;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MidR.Interfaces;
using Modules.Users.Application.Abstractions;
using Newtonsoft.Json;

namespace Modules.Users.Infrastructure.Inbox
{
    internal sealed class InboxProcessor(
        ILogger<InboxProcessor> logger,
        IOptions<InboxOptions> options,
        IServiceProvider serviceProvider
        ) : BackgroundService
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
                    await ProcessInboxAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[Users] Unhandled exception in inbox worker");
                }
            }
        }

        private async Task ProcessInboxAsync(CancellationToken stoppingToken)
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUsersUnitOfWork>();
            var inboxRepository = scope.ServiceProvider.GetRequiredService<IUsersInboxRepository>();

            logger.LogInformation("[Users] Beginning to process inbox messages");

            await unitOfWork.BeginTransactionAsync(stoppingToken);

            var inboxMessages = await inboxRepository.GetAsync(
                _inboxOptions.BatchSize,
                stoppingToken);

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
                    await messagePublisher.PublishAsync(integrationEvent, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[Users] Exception while processing inbox message {MessageId}", inboxMessage.Id);
                    exception = ex;
                }

                ApplyRetryPolicy(inboxMessage, exception);

                await inboxRepository.UpdateAsync(
                    exception,
                    inboxMessage,
                    stoppingToken);
            }

            await unitOfWork.CommitAsync(stoppingToken);

            logger.LogInformation("[Users] Completed process inbox messages");
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
                    "[Users] Permanent failure on inbox message {MessageId}: {Error}",
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
                    "[Users] Inbox message {MessageId} exceeded max retries ({MaxRetry}). Marked as permanent failure.",
                    inboxMessage.Id,
                    _inboxOptions.MaxRetryCount);

                return;
            }

            inboxMessage.NextRetryAt = DateTimeOffset.UtcNow.ComputeNextRetryAt(inboxMessage.RetryCount);

            logger.LogWarning(
                "[Users] Inbox message {MessageId} scheduled for retry {RetryCount}/{MaxRetry} at {NextRetryAt}",
                inboxMessage.Id,
                inboxMessage.RetryCount,
                _inboxOptions.MaxRetryCount,
                inboxMessage.NextRetryAt);
        }

    }
}
