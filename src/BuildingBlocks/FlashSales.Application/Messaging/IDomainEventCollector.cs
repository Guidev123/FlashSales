using FlashSales.Domain.DomainObjects;

namespace FlashSales.Application.Messaging
{
    public interface IDomainEventCollector
    {
        void Collect(Entity entity);

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

        public IReadOnlyList<DomainEvent> Flush()
        {
            var events = _events.ToList();
            _events.Clear();

            return events;
        }
    }
}