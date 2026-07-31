using FlashSales.Infrastructure.Database;
using Modules.Orders.Application.Abstractions;

namespace Modules.Orders.Infrastructure.Database.Repositories
{
    internal sealed class UnitOfWork(OrdersDbContext context)
        : BaseUnitOfWork<OrdersDbContext>(context), IOrdersUnitOfWork;
}
