using Dapper;
using FlashSales.Application.Extensions;
using FlashSales.Application.Outbox;
using FlashSales.Domain.DomainObjects;
using FlashSales.Infrastructure.Extensions;
using Modules.Catalog.Application.Abstractions;
using Newtonsoft.Json;

namespace Modules.Catalog.Infrastructure.Database.Repositories
{
    internal sealed class OutboxRepository(ICatalogUnitOfWork unitOfWork) : IOutboxRepository
    {
        public Task InsertAsync(DomainEvent domainEvent, CancellationToken cancellationToken)
        {
            const string sql = """
                INSERT INTO catalog."OutboxMessages"("Id", "CorrelationId", "Type", "Content", "OccurredOn", "RetryCount", "IsPermanentFailure")
                VALUES(@Id, @CorrelationId, @Type, @Content, @OccurredOn, 0, false)
                """;

            return unitOfWork.Connection.ExecuteAsync(unitOfWork.CreateCommand(sql, new
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
                FROM catalog."OutboxMessages"
                WHERE "ProcessedOn" IS NULL
                  AND "IsPermanentFailure" = false
                  AND ("NextRetryAt" IS NULL OR "NextRetryAt" <= @Now)
                ORDER BY "OccurredOn"
                LIMIT @BatchSize
                """;

            var result = await unitOfWork.Connection.QueryAsync<OutboxMessage>(
                unitOfWork.CreateCommand(sql, new { Now = DateTimeOffset.UtcNow, BatchSize = batchSize }, cancellationToken)).WaitAsync(cancellationToken);

            return result.ToList();
        }

        public Task UpdateAsync(Exception? exception, OutboxMessage outboxMessage, CancellationToken cancellationToken)
        {
            const string sql = """
                UPDATE catalog."OutboxMessages"
                SET "ProcessedOn"        = @ProcessedOn,
                    "Error"              = @Error,
                    "RetryCount"         = @RetryCount,
                    "NextRetryAt"        = @NextRetryAt,
                    "IsPermanentFailure" = @IsPermanentFailure
                WHERE "Id" = @Id
                """;

            return unitOfWork.Connection.ExecuteAsync(unitOfWork.CreateCommand(sql, new
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
                    SELECT 1 FROM catalog."OutboxMessageConsumers"
                    WHERE "OutboxMessageId" = @OutboxMessageId
                      AND "Name" = @Name
                )
                """;

            return unitOfWork.Connection.ExecuteScalarAsync<bool>(
                unitOfWork.CreateCommand(sql, new { outboxMessageConsumer.OutboxMessageId, outboxMessageConsumer.Name }, cancellationToken)).WaitAsync(cancellationToken);
        }

        public Task MarkAsProcessedAsync(OutboxMessageConsumer outboxMessageConsumer, CancellationToken cancellationToken)
        {
            const string sql = """
                INSERT INTO catalog."OutboxMessageConsumers" ("Id", "OutboxMessageId", "Name")
                VALUES (@Id, @OutboxMessageId, @Name)
                ON CONFLICT DO NOTHING
                """;

            return unitOfWork.Connection.ExecuteAsync(
                unitOfWork.CreateCommand(sql, new
                {
                    outboxMessageConsumer.Id,
                    outboxMessageConsumer.OutboxMessageId,
                    outboxMessageConsumer.Name
                }, cancellationToken)).WaitAsync(cancellationToken);
        }
    }
}