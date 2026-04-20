namespace FlashSales.Domain.DomainObjects
{
    public abstract class Entity
    {
        private readonly List<DomainEvent> _domainEvents = [];

        protected Entity()
        {
            Id = Guid.NewGuid();
            CreatedOn = DateTimeOffset.UtcNow;
        }

        public Guid Id { get; protected set; }
        public DateTimeOffset CreatedOn { get; }
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        public void AddDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }

        public void RemoveDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        protected abstract void Validate();

        public override bool Equals(object? obj)
        {
            var compareTo = obj as Entity;

            if (ReferenceEquals(this, compareTo)) return true;
            if (compareTo is null) return false;

            return Id.Equals(compareTo.Id);
        }

        public static bool operator ==(Entity a, Entity b)
        {
            if (a is null && b is null) return true;
            if (!(a is not null && b is not null)) return false;

            return a.Equals(b);
        }

        public static bool operator !=(Entity a, Entity b) => !(a == b);

        public override int GetHashCode() => (GetType().GetHashCode() * 907) + Id.GetHashCode();

        public override string ToString() => $"{GetType().Name} [Id = {Id}]";
    }
}