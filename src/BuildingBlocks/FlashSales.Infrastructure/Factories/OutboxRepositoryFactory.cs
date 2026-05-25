using FlashSales.Application.Outbox;

namespace FlashSales.Infrastructure.Factories
{
    internal sealed class OutboxRepositoryFactory(
        IEnumerable<IOutboxRepositoryRegistration> registrations,
        IServiceProvider sp
        ) : IOutboxRepositoryFactory
    {
        public IOutboxRepository Create(Type commandType)
        {
            var registration = registrations.FirstOrDefault(r => r.Matches(commandType))
                ?? throw new InvalidOperationException(
                    $"No {nameof(IOutboxRepository)} registered for command '{commandType.Name}'. " +
                    $"Ensure the module owning this command has registered an {nameof(IOutboxRepositoryRegistration)}.");

            return registration.Resolve(sp);
        }
    }
}