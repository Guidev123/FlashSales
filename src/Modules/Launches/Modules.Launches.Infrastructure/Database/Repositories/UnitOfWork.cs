using FlashSales.Infrastructure.Database;
using Modules.Launches.Application.Abstractions;

namespace Modules.Launches.Infrastructure.Database.Repositories
{
    internal sealed class UnitOfWork(LaunchesDbContext context)
        : BaseUnitOfWork<LaunchesDbContext>(context), ILaunchesUnitOfWork;
}
