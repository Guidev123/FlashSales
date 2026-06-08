using System.Data;

namespace FlashSales.Application.Abstractions
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);

        Task<CommitResult> CommitAsync(CancellationToken cancellationToken = default);

        Task<CommitResult> SaveChangesAsync(CancellationToken cancellationToken = default);

        Task RollbackAsync(CancellationToken cancellationToken = default);

        IDbTransaction? Transaction { get; }
        IDbConnection Connection { get; }
    }
}