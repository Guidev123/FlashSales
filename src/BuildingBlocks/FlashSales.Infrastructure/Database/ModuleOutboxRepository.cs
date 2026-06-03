using FlashSales.Application.Abstractions;

namespace FlashSales.Infrastructure.Database
{
    public sealed class ModuleOutboxRepository<TUnitOfWork>(TUnitOfWork unitOfWork, string schema)
        : BaseOutboxRepository(unitOfWork, schema)
        where TUnitOfWork : IUnitOfWork;
}
