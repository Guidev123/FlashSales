using Microsoft.EntityFrameworkCore;
using Modules.Payments.Domain.Payments.Entities;
using Modules.Payments.Domain.Payments.Enums;
using Modules.Payments.Domain.Payments.Repositories;

namespace Modules.Payments.Infrastructure.Database.Repositories
{
    internal sealed class PaymentRepository(PaymentsDbContext context) : IPaymentRepository
    {
        public void Add(Payment payment)
        {
            context.Payments.Add(payment);
        }

        public void Update(Payment payment)
        {
            var paymentEntry = context.Entry(payment);

            if (paymentEntry.State == EntityState.Added)
                return;

            foreach (var attempt in payment.Attempts)
            {
                var entry = context.Entry(attempt);
                if (entry.State != EntityState.Unchanged)
                    entry.State = EntityState.Added;
            }

            paymentEntry.State = EntityState.Modified;
        }

        public Task<Payment?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default)
        {
            return context.Payments
                .Include(p => p.Attempts)
                .FirstOrDefaultAsync(p => p.OrderId == orderId, cancellationToken);
        }

        public Task<Payment?> GetByAttemptIdAsync(Guid attemptId, CancellationToken cancellationToken = default)
        {
            return context.Payments
                .Include(p => p.Attempts)
                .FirstOrDefaultAsync(p => p.Attempts.Any(a => a.Id == attemptId), cancellationToken);
        }

        public async Task<IReadOnlyCollection<Guid>> GetStaleInitiatedAttemptIdsAsync(TimeSpan staleness, CancellationToken cancellationToken = default)
        {
            var cutoff = DateTimeOffset.UtcNow - staleness;

            return await context.Payments
                .AsNoTracking()
                .SelectMany(p => p.Attempts)
                .Where(a =>
                    (a.Status == PaymentAttemptStatus.Initiated && a.CreatedOn < cutoff) ||
                    a.Status == PaymentAttemptStatus.TimedOut)
                .Select(a => a.Id)
                .ToListAsync(cancellationToken);
        }
    }
}