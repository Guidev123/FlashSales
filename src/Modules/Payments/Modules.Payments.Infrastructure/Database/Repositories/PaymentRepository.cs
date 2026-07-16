using Modules.Payments.Domain.Payments.Repositories;

namespace Modules.Payments.Infrastructure.Database.Repositories
{
    internal sealed class PaymentRepository(PaymentsDbContext context) : IPaymentRepository
    {
    }
}
