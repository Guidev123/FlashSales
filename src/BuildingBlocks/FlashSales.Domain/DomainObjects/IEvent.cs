using MidR.Interfaces;

namespace FlashSales.Domain.DomainObjects
{
    public interface IEvent : INotification
    {
        Guid CorrelationId { get; }
        DateTimeOffset OccurredOn { get; }
        string MessageType { get; }
    }
}