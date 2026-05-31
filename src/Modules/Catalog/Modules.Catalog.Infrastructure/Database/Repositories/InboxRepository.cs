using FlashSales.Infrastructure.Database;
using Modules.Catalog.Application.Abstractions;
using Modules.Catalog.Infrastructure.Inbox;

namespace Modules.Catalog.Infrastructure.Database.Repositories
{
    internal sealed class InboxRepository(ICatalogUnitOfWork unitOfWork)
        : BaseInboxRepository(unitOfWork, "catalog"), ICatalogInboxRepository;
}
