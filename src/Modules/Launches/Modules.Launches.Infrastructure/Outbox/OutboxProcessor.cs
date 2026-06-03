using FlashSales.Application.Abstractions;
using FlashSales.Application.Outbox;
using FlashSales.Infrastructure.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Launches.Application.Abstractions;

namespace Modules.Launches.Infrastructure.Outbox
{
    internal sealed class OutboxProcessor(
        ILogger<OutboxProcessor> logger,
        IOptions<OutboxOptions> options,
        IServiceProvider serviceProvider
        ) : BaseOutboxProcessor(logger, options, serviceProvider, "Launches")
    {
        protected override IUnitOfWork GetUnitOfWork(IServiceProvider sp)
            => sp.GetRequiredService<ILaunchesUnitOfWork>();

        protected override IOutboxRepository GetOutboxRepository(IServiceProvider sp)
            => sp.GetRequiredService<ILaunchesOutboxRepository>();
    }
}
