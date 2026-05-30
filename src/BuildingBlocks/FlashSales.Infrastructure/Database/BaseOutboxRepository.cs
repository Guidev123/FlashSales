using Dapper;
using FlashSales.Application.Abstractions;
using FlashSales.Application.Extensions;
using FlashSales.Application.Outbox;
using FlashSales.Domain.DomainObjects;
using FlashSales.Infrastructure.Extensions;
using Newtonsoft.Json;

namespace FlashSales.Infrastructure.Database
{
    public abstract class BaseOutboxRepository(IUnitOfWork unitOfWork, string schema) : IOutboxRepository
    {
        public Task InsertAsync(DomainEvent domainEvent, CancellationToken cancellationToken)
        {
            var sql = $"""
                INSERT INTO {schema}."OutboxMessages"("Id", "CorrelationId", "Type", "Content", "OccurredOn", "RetryCount", "IsPermanentFailure")
                VALUES(@Id, @CorrelationId, @Type, @Content::jsonb, @OccurredOn, 0, false)
                """;

            return unitOfWork.Connection.ExecuteAsync(unitOfWork.CreateCommand(sql, new
            {
                Id = Guid.NewGuid(),
                domainEvent.CorrelationId,
                Type = domainEvent.GetType().AssemblyQualifiedName!,
                Content = JsonConvert.SerializeObject(domainEvent, JsonSerializerSettingsExtensions.Instance),
                OccurredOn = domainEvent.OccurredOn.ToUniversalTime()
            }, cancellationToken)).WaitAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<OutboxMessage>> GetAsync(int batchSize, CancellationToken cancellationToken)
        {
            var sql = $"""
                SELECT "Id", "CorrelationId", "Type", "Content"::text AS "Content", "OccurredOn",
                       "ProcessedOn", "Error", "RetryCount", "NextRetryAt", "IsPermanentFailure"
                FROM {schema}."OutboxMessages"
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
            var sql = $"""
                UPDATE {schema}."OutboxMessages"
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

        public Task<bool> IsProcessedAsync(Guid correlationId, string name, CancellationToken cancellationToken)
        {
            var sql = $"""
                SELECT EXISTS (
                    SELECT 1 FROM {schema}."OutboxMessageConsumers" omc
                    WHERE omc."OutboxMessageId" = (
                        SELECT "Id" FROM {schema}."OutboxMessages" WHERE "CorrelationId" = @CorrelationId
                    )
                    AND omc."Name" = @Name
                )
                """;

            return unitOfWork.Connection.ExecuteScalarAsync<bool>(
                unitOfWork.CreateCommand(sql, new { CorrelationId = correlationId, Name = name }, cancellationToken)).WaitAsync(cancellationToken);
        }

        public Task MarkAsProcessedAsync(Guid correlationId, string name, CancellationToken cancellationToken)
        {
            var sql = $"""
                INSERT INTO {schema}."OutboxMessageConsumers" ("Id", "OutboxMessageId", "Name")
                SELECT @Id, "Id", @Name
                FROM {schema}."OutboxMessages"
                WHERE "CorrelationId" = @CorrelationId
                ON CONFLICT DO NOTHING
                """;

            return unitOfWork.Connection.ExecuteAsync(
                unitOfWork.CreateCommand(sql, new
                {
                    Id = Guid.NewGuid(),
                    CorrelationId = correlationId,
                    Name = name
                }, cancellationToken)).WaitAsync(cancellationToken);
        }
    }
}
