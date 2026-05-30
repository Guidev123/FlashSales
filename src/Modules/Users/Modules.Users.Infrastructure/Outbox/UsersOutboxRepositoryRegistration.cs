using FlashSales.Application.Outbox;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.Application;
using Modules.Users.Infrastructure.Database.Repositories;

namespace Modules.Users.Infrastructure.Outbox
{
    internal sealed class UsersOutboxRepositoryRegistration : IOutboxRepositoryRegistration
    {
        public bool Matches(Type commandType)
            => commandType.Assembly == AssemblyReference.Assembly;

        public IOutboxRepository Resolve(IServiceProvider sp)
            => sp.GetRequiredService<OutboxRepository>();
    }
}
