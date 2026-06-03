using FlashSales.Application.Abstractions;

namespace FlashSales.Infrastructure.Database
{
    public sealed class ModuleInboxRepository<TUnitOfWork>(TUnitOfWork unitOfWork, string schema)
        : BaseInboxRepository(unitOfWork, schema)
        where TUnitOfWork : IUnitOfWork;
}
