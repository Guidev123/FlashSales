using FlashSales.Infrastructure.Database;
using Modules.Catalog.Application.Abstractions;

namespace Modules.Catalog.Infrastructure.Database.Repositories
{
    internal sealed class UnitOfWork(CatalogDbContext context)
        : BaseUnitOfWork<CatalogDbContext>(context), ICatalogUnitOfWork;
}
