using FlashSales.Application.Abstractions;

namespace FlashSales.Infrastructure.Factories
{
    internal sealed class UnitOfWorkFactory(
        IEnumerable<IUnitOfWorkRegistration> registrations,
        IServiceProvider sp
        ) : IUnitOfWorkFactory
    {
        public IUnitOfWork Create(Type commandType)
        {
            var registration = registrations.FirstOrDefault(r => r.Matches(commandType))
                ?? throw new InvalidOperationException(
                    $"No {nameof(IUnitOfWork)} registered for command '{commandType.Name}'. " +
                    $"Ensure the module owning this command has registered an {nameof(IUnitOfWorkRegistration)}.");

            return registration.Resolve(sp);
        }
    }
}