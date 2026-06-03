using FlashSales.Infrastructure.Database;
using Modules.Launches.Application.Abstractions;
using Modules.Launches.Infrastructure.Outbox;

namespace Modules.Launches.Infrastructure.Database.Repositories
{
    internal sealed class OutboxRepository(ILaunchesUnitOfWork unitOfWork)
        : BaseOutboxRepository(unitOfWork, Schemas.Launches), ILaunchesOutboxRepository;
}
