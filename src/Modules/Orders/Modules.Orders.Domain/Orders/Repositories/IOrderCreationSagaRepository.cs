using Modules.Orders.Domain.Orders.Models;

namespace Modules.Orders.Domain.Orders.Repositories
{
    public interface IOrderCreationSagaRepository
    {
        Task<OrderCreationSaga?> GetByIdAsync(Guid sagaId, CancellationToken cancellationToken = default);

        void Add(OrderCreationSaga saga);

        void Update(OrderCreationSaga saga);

        Task<IReadOnlyCollection<Guid>> GetStaleAsync(TimeSpan staleness, CancellationToken cancellationToken = default);
    }
}
