using FlashSales.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Modules.Users.Infrastructure.Database.Repositories
{
    internal sealed class UnitOfWork(
        UsersDbContext context
    ) : IUnitOfWork, IDisposable, IAsyncDisposable
    {
        public IDbConnection Connection => context.Database.GetDbConnection();
        public IDbTransaction? Transaction => _contextTransaction?.GetDbTransaction();

        private IDbContextTransaction? _contextTransaction;
        private int _nestingLevel;

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_contextTransaction is not null)
            {
                _nestingLevel++;
                return;
            }

            _contextTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
            _nestingLevel = 1;
        }

        public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_nestingLevel > 1)
            {
                _nestingLevel--;
                return true;
            }

            try
            {
                await _contextTransaction!.CommitAsync(cancellationToken);
                _nestingLevel = 0;
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
            _nestingLevel = 0;

            if (_contextTransaction is not null)
                await _contextTransaction.RollbackAsync(cancellationToken);
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => context.SaveChangesAsync(cancellationToken);

        public async ValueTask DisposeAsync()
        {
            if (_contextTransaction is not null)
                await _contextTransaction.DisposeAsync();

            await context.DisposeAsync();
        }

        public void Dispose()
        {
            _contextTransaction?.Dispose();
            context?.Dispose();
        }
    }
}