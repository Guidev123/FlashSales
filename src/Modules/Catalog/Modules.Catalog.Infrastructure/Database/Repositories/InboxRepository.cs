using FlashSales.Infrastructure.Database;
using Modules.Catalog.Application.Abstractions;

namespace Modules.Catalog.Infrastructure.Database.Repositories
{
    internal sealed class InboxRepository(ICatalogUnitOfWork unitOfWork)
        : BaseInboxRepository(unitOfWork, "catalog"), ICatalogInboxRepository;
}
