using FlashSales.Infrastructure.Database;
using Modules.Users.Application.Abstractions;

namespace Modules.Users.Infrastructure.Database.Repositories
{
    internal sealed class InboxRepository(IUsersUnitOfWork unitOfWork)
        : BaseInboxRepository(unitOfWork, "users"), IUsersInboxRepository;
}
