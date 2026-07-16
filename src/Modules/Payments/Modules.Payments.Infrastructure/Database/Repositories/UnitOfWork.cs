using FlashSales.Infrastructure.Database;
using Modules.Payments.Application.Abstractions;

namespace Modules.Payments.Infrastructure.Database.Repositories
{
    internal sealed class UnitOfWork(PaymentsDbContext context)
        : BaseUnitOfWork<PaymentsDbContext>(context), IPaymentsUnitOfWork;
}
