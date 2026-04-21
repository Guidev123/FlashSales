using FlashSales.Domain.DomainObjects;

namespace FlashSales.Application.Messaging
{
    public interface IDomainEventCollector
    {
        void Collect(Entity entity);

        void Collect(DomainEvent domainEvent);

        IReadOnlyList<DomainEvent> Flush();
    }

    public sealed class DomainEventCollector : IDomainEventCollector
    {
        private readonly List<DomainEvent> _events = [];

        public void Collect(Entity entity)
        {
            _events.AddRange(entity.DomainEvents);
            entity.ClearDomainEvents();
        }

        public void Collect(DomainEvent domainEvent) => _events.Add(domainEvent);

        public IReadOnlyList<DomainEvent> Flush()
        {
            var events = _events.ToList();
            _events.Clear();

            return events;
        }
    }
}