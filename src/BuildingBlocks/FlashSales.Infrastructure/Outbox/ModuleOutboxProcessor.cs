using FlashSales.Application.Abstractions;
using FlashSales.Application.Outbox;
using FlashSales.Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlashSales.Infrastructure.Outbox
{
    public sealed class ModuleOutboxProcessor<TUnitOfWork>(
        ILogger<ModuleOutboxProcessor<TUnitOfWork>> logger,
        IOptionsMonitor<OutboxOptions> options,
        IServiceProvider serviceProvider,
        string moduleName
    ) : BaseOutboxProcessor(logger, options, serviceProvider, moduleName)
        where TUnitOfWork : IUnitOfWork
    {
        protected override IUnitOfWork GetUnitOfWork(IServiceProvider sp)
            => sp.GetRequiredService<TUnitOfWork>();

        protected override IOutboxRepository GetOutboxRepository(IServiceProvider sp)
            => sp.GetRequiredService<ModuleOutboxRepository<TUnitOfWork>>();
    }
}
