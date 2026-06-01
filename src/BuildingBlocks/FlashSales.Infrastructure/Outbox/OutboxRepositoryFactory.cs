using FlashSales.Application.Outbox;
using System.Collections.Concurrent;

namespace FlashSales.Infrastructure.Outbox
{
    internal sealed class OutboxRepositoryFactory(
        IEnumerable<IOutboxRepositoryRegistration> registrations,
        IServiceProvider sp
        ) : IOutboxRepositoryFactory
    {
        private static readonly ConcurrentDictionary<Type, IOutboxRepositoryRegistration> _cache = new();

        public IOutboxRepository Create(Type commandType)
        {
            var registration = _cache.GetOrAdd(commandType, t =>
                registrations.FirstOrDefault(r => r.Matches(t))
                ?? throw new InvalidOperationException(
                    $"No {nameof(IOutboxRepository)} registered for command '{t.Name}'. " +
                    $"Ensure the module owning this command has registered an {nameof(IOutboxRepositoryRegistration)}."));

            return registration.Resolve(sp);
        }
    }
}