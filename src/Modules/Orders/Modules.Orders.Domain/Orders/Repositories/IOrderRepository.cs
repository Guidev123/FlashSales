using Modules.Orders.Domain.Orders.Entities;

namespace Modules.Orders.Domain.Orders.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        void Add(Order order);

        void Update(Order order);

        Task<IReadOnlyCollection<Guid>> GetStaleAwaitingOrProcessingAsync(CancellationToken cancellationToken = default);
    }
}
