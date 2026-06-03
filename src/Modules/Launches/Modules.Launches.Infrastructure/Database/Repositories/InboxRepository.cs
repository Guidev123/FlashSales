using FlashSales.Infrastructure.Database;
using Modules.Launches.Application.Abstractions;
using Modules.Launches.Infrastructure.Inbox;

namespace Modules.Launches.Infrastructure.Database.Repositories
{
    internal sealed class InboxRepository(ILaunchesUnitOfWork unitOfWork)
        : BaseInboxRepository(unitOfWork, Schemas.Launches), ILaunchesInboxRepository;
}
