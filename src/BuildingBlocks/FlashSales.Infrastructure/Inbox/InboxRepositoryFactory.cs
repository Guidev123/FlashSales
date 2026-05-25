using FlashSales.Application.Inbox;

namespace FlashSales.Infrastructure.Inbox
{
    internal sealed class InboxRepositoryFactory(
        IEnumerable<IInboxRepositoryRegistration> registrations,
        IServiceProvider sp
        ) : IInboxRepositoryFactory
    {
        public IInboxRepository Create(Type commandType)
        {
            var registration = registrations.FirstOrDefault(r => r.Matches(commandType))
                ?? throw new InvalidOperationException(
                    $"No {nameof(IInboxRepository)} registered for command '{commandType.Name}'. " +
                    $"Ensure the module owning this command has registered an {nameof(IInboxRepositoryRegistration)}.");

            return registration.Resolve(sp);
        }
    }
}