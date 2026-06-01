using FlashSales.Application.Abstractions;
using FlashSales.Application.Inbox;
using FlashSales.Infrastructure.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Users.Application.Abstractions;
using Modules.Users.Infrastructure.Inbox;

namespace Modules.Users.Infrastructure.Inbox
{
    internal sealed class InboxProcessor(
        ILogger<InboxProcessor> logger,
        IOptions<InboxOptions> options,
        IServiceProvider serviceProvider
        ) : BaseInboxProcessor(logger, options, serviceProvider, "Users")
    {
        protected override IUnitOfWork GetUnitOfWork(IServiceProvider sp)
            => sp.GetRequiredService<IUsersUnitOfWork>();

        protected override IInboxRepository GetInboxRepository(IServiceProvider sp)
            => sp.GetRequiredService<IUsersInboxRepository>();
    }
}
