using FlashSales.Infrastructure.Database;
using Modules.Users.Application.Abstractions;

namespace Modules.Users.Infrastructure.Database.Repositories
{
    internal sealed class OutboxRepository(IUsersUnitOfWork unitOfWork)
        : BaseOutboxRepository(unitOfWork, "users"), IUsersOutboxRepository;
}
