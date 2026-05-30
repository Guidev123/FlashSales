using Azure.Messaging.ServiceBus;
using FlashSales.Application.Bus;
using FlashSales.Application.Messaging;
using Microsoft.Extensions.Logging;
using MidR.Interfaces;

namespace FlashSales.Infrastructure.Bus
{
    internal sealed class AzureServiceBus : IEventBus, IAsyncDisposable
    {
        private readonly SenderPool _senderPool;
        private readonly ILogger<AzureServiceBus> _logger;
        private readonly ServiceBusClient _client;
        private readonly Lock _processorsLock = new();
        private readonly List<ServiceBusProcessor> _processors = [];

        public AzureServiceBus(
            ServiceBusClient client,
            ILogger<AzureServiceBus> logger)
        {
            _senderPool = new SenderPool(client);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _client = client ?? throw new ArgumentNullException(nameof(client));
        }

        public Task PublishAsync(
            string topicName,
            IntegrationEnvelope envelope,
            CancellationToken cancellationToken = default)
        {
            var sbMessage = ServiceBusMessageFactory.Create(envelope);
            var sender = _senderPool.GetOrCreate(topicName);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                "Publishing {MessageType} to topic '{Topic}' [{MessageId}]",
                envelope.MessageType, topicName, envelope.MessageId);
            }

            return sender.SendMessageAsync(sbMessage, cancellationToken);
        }

        public Task SendAsync(
            string queueName,
            IntegrationEnvelope envelope,
            CancellationToken cancellationToken = default)
        {
            var sbMessage = ServiceBusMessageFactory.Create(envelope);
            var sender = _senderPool.GetOrCreate(queueName);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                "Sending {MessageType} to queue '{Queue}' [{MessageId}]",
                envelope.MessageType, queueName, envelope.MessageId);
            }

            return sender.SendMessageAsync(sbMessage, cancellationToken);
        }

        public Task ScheduleAsync(
            string topicName,
            IntegrationEnvelope envelope,
            DateTimeOffset scheduledEnqueueTime,
            CancellationToken cancellationToken = default)
        {
            var sbMessage = ServiceBusMessageFactory.Create(envelope);
            var sender = _senderPool.GetOrCreate(topicName);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                    "Scheduling {MessageType} to topic '{Topic}' [{MessageId}] for {ScheduledTime}",
                    envelope.MessageType, topicName, envelope.MessageId, scheduledEnqueueTime);
            }

            return sender.ScheduleMessageAsync(sbMessage, scheduledEnqueueTime, cancellationToken);
        }

        public async Task<IAsyncDisposable> SubscribeAsync(
           string topicName,
           string subscriptionName,
           Func<ConsumedMessage, CancellationToken, Task> handler,
           ConsumerOptions? options = null,
           CancellationToken cancellationToken = default)
        {
            var processorOptions = BuildProcessorOptions(options);
            var processor = _client.CreateProcessor(topicName, subscriptionName, processorOptions);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                "Subscribing to topic '{Topic}' subscription '{Subscription}' " +
                "(MaxConcurrentCalls={MaxConcurrent}, AutoComplete={AutoComplete})",
                topicName, subscriptionName,
                processorOptions.MaxConcurrentCalls,
                processorOptions.AutoCompleteMessages);
            }

            WireHandlers(processor, handler, $"{topicName}/{subscriptionName}");
            TrackProcessor(processor);

            await processor.StartProcessingAsync(cancellationToken);

            return new ProcessorHandle(processor, this);
        }

        public async Task<IAsyncDisposable> ConsumeAsync(
            string queueName,
            Func<ConsumedMessage, CancellationToken, Task> handler,
            ConsumerOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var processorOptions = BuildProcessorOptions(options);
            var processor = _client.CreateProcessor(queueName, processorOptions);

            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(
                "Consuming queue '{Queue}' (MaxConcurrentCalls={MaxConcurrent}, AutoComplete={AutoComplete})",
                queueName,
                processorOptions.MaxConcurrentCalls,
                processorOptions.AutoCompleteMessages);
            }

            WireHandlers(processor, handler, queueName);
            TrackProcessor(processor);

            await processor.StartProcessingAsync(cancellationToken);

            return new ProcessorHandle(processor, this);
        }

        private static ServiceBusProcessorOptions BuildProcessorOptions(ConsumerOptions? options)
        {
            var opts = options ?? new ConsumerOptions();
            return new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = opts.MaxConcurrentCalls,
                AutoCompleteMessages = opts.AutoComplete,
                MaxAutoLockRenewalDuration = opts.MaxAutoLockRenewalDuration,
                PrefetchCount = opts.PrefetchCount,
                ReceiveMode = ServiceBusReceiveMode.PeekLock
            };
        }

        private void WireHandlers(
            ServiceBusProcessor processor,
            Func<ConsumedMessage, CancellationToken, Task> handler,
            string source)
        {
            processor.ProcessMessageAsync += async args =>
            {
                var received = args.Message;

                var messageType = received.Subject
                    ?? (received.ApplicationProperties.TryGetValue("MessageType", out var mt)
                        ? mt?.ToString() ?? "Unknown"
                        : "Unknown");

                var module = received.ApplicationProperties.TryGetValue("Module", out var mod)
                    ? mod?.ToString() ?? string.Empty
                    : string.Empty;

                var correlationId = Guid.TryParse(received.CorrelationId, out var cid)
                    ? cid
                    : Guid.Empty;

                var consumed = new ConsumedMessage
                {
                    MessageId = received.MessageId,
                    MessageType = messageType,
                    CorrelationId = correlationId,
                    Body = received.Body.ToMemory(),
                    Properties = received.ApplicationProperties.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                    DeliveryCount = received.DeliveryCount,
                    EnqueuedTime = received.EnqueuedTime
                };

                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation(
                        "Received {MessageType} from '{Source}' [{MessageId}] (delivery #{DeliveryCount})",
                        consumed.MessageType, source, consumed.MessageId, consumed.DeliveryCount);
                }

                await handler(consumed, args.CancellationToken);
            };

            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception,
                    "Error processing message from '{Source}' (ErrorSource={ErrorSource}, EntityPath={EntityPath})",
                    source, args.ErrorSource, args.EntityPath);

                return Task.CompletedTask;
            };
        }

        private void TrackProcessor(ServiceBusProcessor processor)
        {
            lock (_processorsLock)
                _processors.Add(processor);
        }

        private void UntrackProcessor(ServiceBusProcessor processor)
        {
            lock (_processorsLock)
                _processors.Remove(processor);
        }

        private sealed class ProcessorHandle(
           ServiceBusProcessor processor,
           AzureServiceBus bus) : IAsyncDisposable
        {
            public async ValueTask DisposeAsync()
            {
                await processor.StopProcessingAsync();
                await processor.DisposeAsync();
                bus.UntrackProcessor(processor);
            }
        }

        public async ValueTask DisposeAsync() =>
            await _senderPool.DisposeAsync();
    }
}