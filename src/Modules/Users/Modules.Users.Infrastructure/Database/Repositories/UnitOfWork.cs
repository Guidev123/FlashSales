using FlashSales.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace Modules.Users.Infrastructure.Database.Repositories
{
    internal sealed class UnitOfWork(
                 UsersDbContext context
             ) : IUnitOfWork, IAsyncDisposable
    {
        private IDbTransaction? _transaction;

        public IDbConnection Connection => context.Database.GetDbConnection();
        public IDbTransaction? Transaction => _transaction;

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (_transaction is not null)
                return;

            await context.Database.OpenConnectionAsync(cancellationToken);

            var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            _transaction = transaction.GetDbTransaction();
        }

        public async Task<bool> CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await context.SaveChangesAsync(cancellationToken);
                await context.Database.CurrentTransaction!.CommitAsync(cancellationToken);
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
            if (context.Database.CurrentTransaction is not null)
            {
                await context.Database.CurrentTransaction.RollbackAsync(cancellationToken);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (context.Database.CurrentTransaction is not null)
                await context.Database.CurrentTransaction.DisposeAsync();

            await context.Database.CloseConnectionAsync();
        }
    }
}