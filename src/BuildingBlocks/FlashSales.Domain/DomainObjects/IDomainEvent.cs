namespace FlashSales.Domain.DomainObjects
{
    internal interface IDomainEvent : IEvent
    {
        Guid AggregateId { get; }
    }
}