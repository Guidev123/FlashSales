using FlashSales.Application.Abstractions;
using FlashSales.Infrastructure.Inbox;
using FlashSales.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FlashSales.Infrastructure.Extensions
{
    public static class EntityFrameworkExtensions
    {
        public static void AddOutboxAndInbox(this ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());
            modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
            modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
            modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
        }

        internal static CommitResult ToCommitResult(this DbUpdateException ex)
        {
            if (ex.InnerException is PostgresException pg)
            {
                return pg.SqlState switch
                {
                    PostgresErrorCodes.UniqueViolation => CommitResult.UniqueViolation(),
                    PostgresErrorCodes.ForeignKeyViolation => CommitResult.ForeignKeyViolation(),
                    _ => CommitResult.ConnectionFailure()
                };
            }

            return CommitResult.ConnectionFailure();
        }
    }
}