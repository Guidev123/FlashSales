using FlashSales.Application.Inbox;
using Microsoft.Extensions.DependencyInjection;
using Modules.Catalog.Infrastructure.Database.Repositories;

namespace Modules.Catalog.Infrastructure.Inbox
{
    internal sealed class CatalogInboxRepositoryRegistration : IInboxRepositoryRegistration
    {
        public bool Matches(Type commandType)
            => commandType.Assembly == Application.AssemblyReference.Assembly
            || commandType.Assembly == Domain.AssemblyReference.Assembly
            || commandType.Assembly == Users.Contracts.AssemblyReference.Assembly;

        public IInboxRepository Resolve(IServiceProvider sp)
            => sp.GetRequiredService<InboxRepository>();
    }
}