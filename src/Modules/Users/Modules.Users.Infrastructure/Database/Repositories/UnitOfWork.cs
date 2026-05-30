using FlashSales.Infrastructure.Database;
using Modules.Users.Application.Abstractions;

namespace Modules.Users.Infrastructure.Database.Repositories
{
    internal sealed class UnitOfWork(UsersDbContext context)
        : BaseUnitOfWork<UsersDbContext>(context), IUsersUnitOfWork;
}
