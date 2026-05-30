using FlashSales.Application.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.Application;
using Modules.Users.Domain;
using Modules.Users.Infrastructure.Database.Repositories;

namespace Modules.Users.Infrastructure.Inbox
{
    internal sealed class UsersInboxRepositoryRegistration : IInboxRepositoryRegistration
    {
        public bool Matches(Type commandType)
            => commandType.Assembly == Application.AssemblyReference.Assembly
            || commandType.Assembly == Domain.AssemblyReference.Assembly;

        public IInboxRepository Resolve(IServiceProvider sp)
            => sp.GetRequiredService<InboxRepository>();
    }
}