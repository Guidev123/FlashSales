using FlashSales.Application.Abstractions;
using FlashSales.Application.Inbox;
using FlashSales.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlashSales.Infrastructure.Inbox
{
    public sealed class ModuleInboxProcessor<TUnitOfWork>(
        ILogger<ModuleInboxProcessor<TUnitOfWork>> logger,
        IOptions<InboxOptions> options,
        IServiceProvider serviceProvider,
        string moduleName
    ) : BaseInboxProcessor(logger, options, serviceProvider, moduleName)
        where TUnitOfWork : IUnitOfWork
    {
        protected override IUnitOfWork GetUnitOfWork(IServiceProvider sp)
            => sp.GetRequiredService<TUnitOfWork>();

        protected override IInboxRepository GetInboxRepository(IServiceProvider sp)
            => sp.GetRequiredService<ModuleInboxRepository<TUnitOfWork>>();
    }
}
