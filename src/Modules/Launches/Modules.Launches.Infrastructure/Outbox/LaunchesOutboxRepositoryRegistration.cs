using FlashSales.Application.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Modules.Launches.Infrastructure.Database.Repositories;

namespace Modules.Launches.Infrastructure.Outbox
{
    internal sealed class LaunchesOutboxRepositoryRegistration : IOutboxRepositoryRegistration
    {
        public bool Matches(Type commandType)
            => LaunchesModule.Assemblies.Contains(commandType.Assembly);

        public IOutboxRepository Resolve(IServiceProvider sp)
            => sp.GetRequiredService<OutboxRepository>();
    }
}
