using FlashSales.Application.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.Infrastructure.Database.Repositories;

namespace Modules.Users.Infrastructure.Inbox
{
    internal sealed class UsersInboxRepositoryRegistration : IInboxRepositoryRegistration
    {
        public bool Matches(Type commandType) => false;

        public IInboxRepository Resolve(IServiceProvider sp)
            => sp.GetRequiredService<InboxRepository>();
    }
}
