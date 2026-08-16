using Modules.Payments.Domain.Payments.Entities;

namespace Modules.Payments.Domain.Payments.Repositories
{
    public interface IPaymentRepository
    {
        void Add(Payment payment);

        void Update(Payment payment);

        Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

        Task<Payment?> GetByAttemptIdAsync(Guid attemptId, CancellationToken cancellationToken = default);

        Task<IReadOnlyCollection<Guid>> GetStaleInitiatedAttemptIdsAsync(TimeSpan staleness, CancellationToken cancellationToken = default);
    }
}