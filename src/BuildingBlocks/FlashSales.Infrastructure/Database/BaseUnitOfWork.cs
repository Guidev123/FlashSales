using FlashSales.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace FlashSales.Infrastructure.Database
{
    public abstract class BaseUnitOfWork<TDbContext>(TDbContext context) : IUnitOfWork
        where TDbContext : DbContext
    {
        public IDbConnection Connection => context.Database.GetDbConnection();
        public IDbTransaction? Transaction => _contextTransaction?.GetDbTransaction();

        private IDbContextTransaction? _contextTransaction;

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            _contextTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _contextTransaction!.CommitAsync(cancellationToken);
                await _contextTransaction.DisposeAsync();
                _contextTransaction = null;
                return true;
            }
            catch
            {
                await RollbackAsync(cancellationToken);
                return false;
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_contextTransaction is null) return;
            await _contextTransaction.RollbackAsync(cancellationToken);
            await _contextTransaction.DisposeAsync();
            _contextTransaction = null;
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => context.SaveChangesAsync(cancellationToken);

        public async ValueTask DisposeAsync()
        {
            if (_contextTransaction is not null) await _contextTransaction.DisposeAsync();
            await context.DisposeAsync();
        }

        public void Dispose()
        {
            _contextTransaction?.Dispose();
        }
    }
}
