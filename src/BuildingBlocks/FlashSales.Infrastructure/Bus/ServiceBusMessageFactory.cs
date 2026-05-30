using Azure.Messaging.ServiceBus;
using FlashSales.Application.Bus;
using System.Diagnostics;

namespace FlashSales.Infrastructure.Bus
{
    internal static class ServiceBusMessageFactory
    {
        public static ServiceBusMessage Create(IntegrationEnvelope envelope)
        {
            var message = new ServiceBusMessage(envelope.Body)
            {
                MessageId = envelope.MessageId,
                ContentType = envelope.ContentType,
                Subject = envelope.MessageType,
                CorrelationId = envelope.CorrelationId.ToString()
            };

            if (envelope.SessionId is not null)
            {
                message.SessionId = envelope.SessionId;
            }

            message.ApplicationProperties["MessageType"] = envelope.MessageType;
            message.ApplicationProperties["Module"] = envelope.Module;

            if (envelope.Properties is not null)
            {
                foreach (var (key, value) in envelope.Properties)
                    message.ApplicationProperties[key] = value;
            }

            if (Activity.Current is { } activity)
            {
                message.ApplicationProperties["Diagnostic-Id"] = activity.Id;
            }

            return message;
        }
    }
}