using FlashSales.Application.Abstractions;
using FlashSales.Application.Inbox;
using FlashSales.Infrastructure.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Launches.Application.Abstractions;

namespace Modules.Launches.Infrastructure.Inbox
{
    internal sealed class InboxProcessor(
        ILogger<InboxProcessor> logger,
        IOptions<InboxOptions> options,
        IServiceProvider serviceProvider
        ) : BaseInboxProcessor(logger, options, serviceProvider, "Launches")
    {
        protected override IUnitOfWork GetUnitOfWork(IServiceProvider sp)
            => sp.GetRequiredService<ILaunchesUnitOfWork>();

        protected override IInboxRepository GetInboxRepository(IServiceProvider sp)
            => sp.GetRequiredService<ILaunchesInboxRepository>();
    }
}
