using Dapper;
using FlashSales.Application.Extensions;
using FlashSales.Application.Outbox;
using FlashSales.Domain.DomainObjects;
using Modules.Users.Application.Abstractions;
using Newtonsoft.Json;

namespace Modules.Users.Infrastructure.Database.Repositories
{
    internal sealed class OutboxRepository(IUsersUnitOfWork unitOfWork) : IOutboxRepository
    {
        private CommandDefinition Cmd(string sql, object? param = null, CancellationToken cancellationToken = default) =>
            new(sql, param, transaction: unitOfWork.Transaction, cancellationToken: cancellationToken);

        public Task InsertAsync(DomainEvent domainEvent, CancellationToken cancellationToken)
        {
            const string sql = """
                INSERT INTO users."OutboxMessages"("Id", "CorrelationId", "Type", "Content", "OccurredOn", "RetryCount", "IsPermanentFailure")
                VALUES(@Id, @CorrelationId, @Type, @Content, @OccurredOn, 0, false)
                """;

            return unitOfWork.Connection.ExecuteAsync(Cmd(sql, new
            {
                Id = Guid.NewGuid(),
                domainEvent.CorrelationId,
                Type = domainEvent.GetType().AssemblyQualifiedName!,
                Content = JsonConvert.SerializeObject(domainEvent, JsonSerializerSettingsExtensions.Instance),
                domainEvent.OccurredOn
            }, cancellationToken)).WaitAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<OutboxMessage>> GetAsync(int batchSize, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT "Id", "CorrelationId", "Type", "Content", "OccurredOn",
                       "ProcessedOn", "Error", "RetryCount", "NextRetryAt", "IsPermanentFailure"
                FROM users."OutboxMessages"
                WHERE "ProcessedOn" IS NULL
                  AND "IsPermanentFailure" = false
                  AND ("NextRetryAt" IS NULL OR "NextRetryAt" <= @Now)
                ORDER BY "OccurredOn"
                LIMIT @BatchSize
                """;

            var result = await unitOfWork.Connection.QueryAsync<OutboxMessage>(
                Cmd(sql, new { Now = DateTimeOffset.UtcNow, BatchSize = batchSize }, cancellationToken)).WaitAsync(cancellationToken);

            return result.ToList();
        }

        public Task UpdateAsync(Exception? exception, OutboxMessage outboxMessage, CancellationToken cancellationToken)
        {
            const string sql = """
                UPDATE users."OutboxMessages"
                SET "ProcessedOn"       = @ProcessedOn,
                    "Error"             = @Error,
                    "RetryCount"        = @RetryCount,
                    "NextRetryAt"       = @NextRetryAt,
                    "IsPermanentFailure"= @IsPermanentFailure
                WHERE "Id" = @Id
                """;

            return unitOfWork.Connection.ExecuteAsync(Cmd(sql, new
            {
                outboxMessage.Id,
                outboxMessage.ProcessedOn,
                outboxMessage.Error,
                outboxMessage.RetryCount,
                outboxMessage.NextRetryAt,
                outboxMessage.IsPermanentFailure
            }, cancellationToken)).WaitAsync(cancellationToken);
        }

        public Task<bool> IsProcessedAsync(OutboxMessageConsumer outboxMessageConsumer, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT EXISTS (
                    SELECT 1 FROM users."OutboxMessageConsumers"
                    WHERE "OutboxMessageId" = @OutboxMessageId
                      AND "Name" = @Name
                )
                """;

            return unitOfWork.Connection.ExecuteScalarAsync<bool>(
                Cmd(sql, new { outboxMessageConsumer.OutboxMessageId, outboxMessageConsumer.Name }, cancellationToken)).WaitAsync(cancellationToken);
        }

        public Task MarkAsProcessedAsync(OutboxMessageConsumer outboxMessageConsumer, CancellationToken cancellationToken)
        {
            const string sql = """
                INSERT INTO users."OutboxMessageConsumers" ("Id", "OutboxMessageId", "Name")
                VALUES (@Id, @OutboxMessageId, @Name)
                ON CONFLICT DO NOTHING
                """;

            return unitOfWork.Connection.ExecuteAsync(
                Cmd(sql, new
                {
                    outboxMessageConsumer.Id,
                    outboxMessageConsumer.OutboxMessageId,
                    outboxMessageConsumer.Name
                }, cancellationToken)).WaitAsync(cancellationToken);
        }
    }
}