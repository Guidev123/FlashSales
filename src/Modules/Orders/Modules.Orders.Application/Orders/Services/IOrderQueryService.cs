using Modules.Orders.Application.Orders.Dtos;

namespace Modules.Orders.Application.Orders.Services
{
    public interface IOrderQueryService
    {
        Task<OrderResponse?> GetByIdAsync(Guid orderId, Guid customerId, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<OrderResponse>> GetByCustomerAsync(
            Guid customerId,
            int page,
            int size,
            CancellationToken cancellationToken = default);

        Task<int> GetByCustomerTotalCountAsync(Guid customerId, CancellationToken cancellationToken = default);

        Task<bool> HasActiveOrderAsync(Guid customerId, Guid launchId, CancellationToken cancellationToken = default);

        Task<int> GetConfirmedQuantityAsync(Guid customerId, Guid launchId, CancellationToken cancellationToken = default);
    }
}
