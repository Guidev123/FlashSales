using FlashSales.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.Application;
using Modules.Users.Application.Abstractions;

namespace Modules.Users.Infrastructure.Database.Repositories
{
    internal sealed class UsersUnitOfWorkRegistration : IUnitOfWorkRegistration
    {
        public bool Matches(Type commandType)
            => commandType.Assembly == AssemblyReference.Assembly;

        public IUnitOfWork Resolve(IServiceProvider sp)
            => sp.GetRequiredService<IUsersUnitOfWork>();
    }
}