using FlashSales.Application.Abstractions;
using FlashSales.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
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

        public async Task<CommitResult> CommitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _contextTransaction!.CommitAsync(cancellationToken);
                await _contextTransaction.DisposeAsync();
                _contextTransaction = null;
                return CommitResult.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                await RollbackAsync(cancellationToken);
                return CommitResult.ConcurrencyConflict();
            }
            catch (DbUpdateException ex)
            {
                await RollbackAsync(cancellationToken);
                return ex.ToCommitResult();
            }
            catch (NpgsqlException)
            {
                await RollbackAsync(cancellationToken);
                return CommitResult.ConnectionFailure();
            }
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default)
        {
            if (_contextTransaction is null) return;
            await _contextTransaction.RollbackAsync(cancellationToken);
            await _contextTransaction.DisposeAsync();
            _contextTransaction = null;
        }

        public async Task<CommitResult> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await context.SaveChangesAsync(cancellationToken);
                return CommitResult.Success();
            }
            catch (DbUpdateConcurrencyException)
            {
                await RollbackAsync(cancellationToken);
                return CommitResult.ConcurrencyConflict();
            }
            catch (DbUpdateException ex)
            {
                await RollbackAsync(cancellationToken);
                return ex.ToCommitResult();
            }
            catch (NpgsqlException)
            {
                await RollbackAsync(cancellationToken);
                return CommitResult.ConnectionFailure();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_contextTransaction is not null) await _contextTransaction.DisposeAsync();
            await context.DisposeAsync();
            GC.SuppressFinalize(this);
        }

        public void Dispose()
        {
            _contextTransaction?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}