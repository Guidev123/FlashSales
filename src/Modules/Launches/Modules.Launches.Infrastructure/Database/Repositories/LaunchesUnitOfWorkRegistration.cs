using FlashSales.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Modules.Launches.Application.Abstractions;

namespace Modules.Launches.Infrastructure.Database.Repositories
{
    internal sealed class LaunchesUnitOfWorkRegistration : IUnitOfWorkRegistration
    {
        public bool Matches(Type commandType)
            => LaunchesModule.Assemblies.Contains(commandType.Assembly);

        public IUnitOfWork Resolve(IServiceProvider sp)
            => sp.GetRequiredService<ILaunchesUnitOfWork>();
    }
}
