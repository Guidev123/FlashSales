using FlashSales.Application.Extensions;
using FlashSales.Application.Messaging;
using Newtonsoft.Json;
using System.Text;

namespace FlashSales.Application.Bus
{
    public sealed record IntegrationEnvelope
    {
        public string MessageId { get; set; } = null!;

        public string MessageType { get; set; } = null!;

        public string Module { get; set; } = null!;

        public required ReadOnlyMemory<byte> Body { get; set; }

        public string ContentType { get; set; } = "application/json";

        public required Guid CorrelationId { get; set; }

        public string? SessionId { get; set; }

        public IReadOnlyDictionary<string, object>? Properties { get; set; }

        public static IntegrationEnvelope FromEvent(IntegrationEvent @event)
        {
            return new IntegrationEnvelope
            {
                MessageId = @event.CorrelationId.ToString(),
                MessageType = @event.MessageType,
                CorrelationId = @event.CorrelationId,
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event, JsonSerializerSettingsExtensions.Instance))
            };
        }

        public static IntegrationEnvelope FromEvent(
            IntegrationEvent @event,
            string messageId)
        {
            return new IntegrationEnvelope
            {
                MessageId = messageId,
                MessageType = @event.MessageType,
                CorrelationId = @event.CorrelationId,
                Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event, JsonSerializerSettingsExtensions.Instance))
            };
        }
    }
}