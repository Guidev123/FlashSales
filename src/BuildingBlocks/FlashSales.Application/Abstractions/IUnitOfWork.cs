using System.Data;

namespace FlashSales.Application.Abstractions
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task<bool> CommitAsync(CancellationToken cancellationToken = default);

        Task RollbackAsync(CancellationToken cancellationToken = default);

        IDbTransaction? Transaction { get; }
        IDbConnection Connection { get; }
    }
}