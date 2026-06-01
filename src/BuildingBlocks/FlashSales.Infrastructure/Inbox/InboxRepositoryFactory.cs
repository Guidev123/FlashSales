using FlashSales.Application.Inbox;
using System.Collections.Concurrent;

namespace FlashSales.Infrastructure.Inbox
{
    internal sealed class InboxRepositoryFactory(
        IEnumerable<IInboxRepositoryRegistration> registrations,
        IServiceProvider sp
        ) : IInboxRepositoryFactory
    {
        private static readonly ConcurrentDictionary<Type, IInboxRepositoryRegistration> _cache = new();

        public IInboxRepository Create(Type commandType)
        {
            var registration = _cache.GetOrAdd(commandType, t =>
                registrations.FirstOrDefault(r => r.Matches(t))
                ?? throw new InvalidOperationException(
                    $"No {nameof(IInboxRepository)} registered for command '{t.Name}'. " +
                    $"Ensure the module owning this command has registered an {nameof(IInboxRepositoryRegistration)}."));

            return registration.Resolve(sp);
        }
    }
}