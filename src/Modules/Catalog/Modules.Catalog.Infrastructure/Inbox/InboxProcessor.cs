using FlashSales.Application.Abstractions;
using FlashSales.Application.Inbox;
using FlashSales.Infrastructure.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Catalog.Application.Abstractions;

namespace Modules.Catalog.Infrastructure.Inbox
{
    internal sealed class InboxProcessor(
        ILogger<InboxProcessor> logger,
        IOptions<InboxOptions> options,
        IServiceProvider serviceProvider
        ) : BaseInboxProcessor(logger, options, serviceProvider, "Catalog")
    {
        protected override IUnitOfWork GetUnitOfWork(IServiceProvider sp)
            => sp.GetRequiredService<ICatalogUnitOfWork>();

        protected override IInboxRepository GetInboxRepository(IServiceProvider sp)
            => sp.GetRequiredService<ICatalogInboxRepository>();
    }
}
