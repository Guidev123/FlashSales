using Dapper;
using FlashSales.Application.Abstractions;
using FlashSales.Application.Extensions;
using FlashSales.Application.Inbox;
using FlashSales.Application.Messaging;
using FlashSales.Infrastructure.Extensions;
using Newtonsoft.Json;

namespace FlashSales.Infrastructure.Database
{
    public abstract class BaseInboxRepository(IUnitOfWork unitOfWork, string schema) : IInboxRepository
    {
        public Task InsertAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
        {
            var sql = $"""
                INSERT INTO {schema}."InboxMessages"("Id", "CorrelationId", "Type", "Content", "OccurredOn", "RetryCount", "IsPermanentFailure")
                VALUES(@Id, @CorrelationId, @Type, @Content::jsonb, @OccurredOn, 0, false)
                ON CONFLICT ("CorrelationId") DO NOTHING
                """;

            return unitOfWork.Connection.ExecuteAsync(unitOfWork.CreateCommand(sql, new
            {
                Id = Guid.NewGuid(),
                integrationEvent.CorrelationId,
                Type = integrationEvent.GetType().AssemblyQualifiedName!,
                Content = JsonConvert.SerializeObject(integrationEvent, JsonSerializerSettingsExtensions.Instance),
                OccurredOn = integrationEvent.OccurredOn.ToUniversalTime()
            }, cancellationToken)).WaitAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<InboxMessage>> GetAsync(int batchSize, CancellationToken cancellationToken)
        {
            var sql = $"""
                SELECT "Id", "CorrelationId", "Type", "Content"::text AS "Content", "OccurredOn",
                       "ProcessedOn", "Error", "RetryCount", "NextRetryAt", "IsPermanentFailure"
                FROM {schema}."InboxMessages"
                WHERE "ProcessedOn" IS NULL
                  AND "IsPermanentFailure" = false
                  AND ("NextRetryAt" IS NULL OR "NextRetryAt" <= @Now)
                ORDER BY "OccurredOn"
                LIMIT @BatchSize
                FOR UPDATE SKIP LOCKED
                """;

            var result = await unitOfWork.Connection.QueryAsync<InboxMessage>(
                unitOfWork.CreateCommand(sql, new { Now = DateTimeOffset.UtcNow, BatchSize = batchSize }, cancellationToken)).WaitAsync(cancellationToken);

            return result.ToList();
        }

        public Task UpdateAsync(Exception? exception, InboxMessage inboxMessage, CancellationToken cancellationToken)
        {
            var sql = $"""
                UPDATE {schema}."InboxMessages"
                SET "ProcessedOn"        = @ProcessedOn,
                    "Error"              = @Error,
                    "RetryCount"         = @RetryCount,
                    "NextRetryAt"        = @NextRetryAt,
                    "IsPermanentFailure" = @IsPermanentFailure
                WHERE "Id" = @Id
                """;

            return unitOfWork.Connection.ExecuteAsync(unitOfWork.CreateCommand(sql, new
            {
                inboxMessage.Id,
                inboxMessage.ProcessedOn,
                inboxMessage.Error,
                inboxMessage.RetryCount,
                inboxMessage.NextRetryAt,
                inboxMessage.IsPermanentFailure
            }, cancellationToken)).WaitAsync(cancellationToken);
        }

        public Task<bool> IsProcessedAsync(Guid correlationId, string name, CancellationToken cancellationToken)
        {
            var sql = $"""
                SELECT EXISTS (
                    SELECT 1 FROM {schema}."InboxMessageConsumers" imc
                    WHERE imc."InboxMessageId" = (
                        SELECT "Id" FROM {schema}."InboxMessages" WHERE "CorrelationId" = @CorrelationId
                    )
                    AND imc."Name" = @Name
                )
                """;

            return unitOfWork.Connection.ExecuteScalarAsync<bool>(
                unitOfWork.CreateCommand(sql, new { CorrelationId = correlationId, Name = name }, cancellationToken)).WaitAsync(cancellationToken);
        }

        public Task MarkAsProcessedAsync(Guid correlationId, string name, CancellationToken cancellationToken)
        {
            var sql = $"""
                INSERT INTO {schema}."InboxMessageConsumers" ("Id", "InboxMessageId", "Name")
                SELECT @Id, "Id", @Name
                FROM {schema}."InboxMessages"
                WHERE "CorrelationId" = @CorrelationId
                ON CONFLICT ("InboxMessageId", "Name") DO NOTHING
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
