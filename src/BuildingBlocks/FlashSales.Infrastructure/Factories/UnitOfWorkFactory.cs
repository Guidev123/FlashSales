using FlashSales.Application.Abstractions;
using System.Collections.Concurrent;

namespace FlashSales.Infrastructure.Factories
{
    internal sealed class UnitOfWorkFactory(
        IEnumerable<IUnitOfWorkRegistration> registrations,
        IServiceProvider sp
        ) : IUnitOfWorkFactory
    {
        private static readonly ConcurrentDictionary<Type, IUnitOfWorkRegistration> _cache = new();

        public IUnitOfWork Create(Type commandType)
        {
            var registration = _cache.GetOrAdd(commandType, t =>
                registrations.FirstOrDefault(r => r.Matches(t))
                ?? throw new InvalidOperationException(
                    $"No {nameof(IUnitOfWork)} registered for command '{t.Name}'. " +
                    $"Ensure the module owning this command has registered an {nameof(IUnitOfWorkRegistration)}."));

            return registration.Resolve(sp);
        }
    }
}