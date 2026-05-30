using FlashSales.Infrastructure.Database;
using Modules.Catalog.Application.Abstractions;

namespace Modules.Catalog.Infrastructure.Database.Repositories
{
    internal sealed class OutboxRepository(ICatalogUnitOfWork unitOfWork)
        : BaseOutboxRepository(unitOfWork, "catalog"), ICatalogOutboxRepository;
}
