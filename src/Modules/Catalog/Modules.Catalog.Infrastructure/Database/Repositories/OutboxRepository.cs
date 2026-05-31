using FlashSales.Infrastructure.Database;
using Modules.Catalog.Application.Abstractions;
using Modules.Catalog.Infrastructure.Outbox;

namespace Modules.Catalog.Infrastructure.Database.Repositories
{
    internal sealed class OutboxRepository(ICatalogUnitOfWork unitOfWork)
        : BaseOutboxRepository(unitOfWork, "catalog"), ICatalogOutboxRepository;
}
