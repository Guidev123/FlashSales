using FlashSales.Domain.Results;
using MidR.Interfaces;
using Modules.Launches.Application.Launches.Features.ReleaseStock;
using Modules.Launches.Application.Launches.Features.ReserveStock;
using Modules.Launches.Contracts;

namespace Modules.Launches.Infrastructure.PublicApi
{
    internal sealed class LaunchesPublicApi(ISender sender) : ILaunchesPublicApi
    {
        public Task<Result> ReleaseAsync(Guid launchId, int quantity, Guid orderId, CancellationToken cancellationToken = default)
        {
            return sender.SendAsync(new ReleaseStockCommand(launchId, orderId, quantity), cancellationToken);
        }

        public Task<Result> ReserveAsync(Guid launchId, int quantity, Guid orderId, CancellationToken cancellationToken = default)
        {
            return sender.SendAsync(new ReserveStockCommand(launchId, orderId, quantity), cancellationToken);
        }
    }
}