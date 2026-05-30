using Dapper;
using FlashSales.Application.Extensions;
using FlashSales.Application.Inbox;
using FlashSales.Application.Messaging;
using FlashSales.Infrastructure.Extensions;
using Modules.Users.Application.Abstractions;
using Newtonsoft.Json;

namespace Modules.Users.Infrastructure.Database.Repositories
{
    internal sealed class InboxRepository(IUsersUnitOfWork unitOfWork) : IInboxRepository
    {
        public Task InsertAsync(IntegrationEvent integrationEvent, CancellationToken cancellationToken)
        {
            const string sql = """
                INSERT INTO users."InboxMessages"("Id", "CorrelationId", "Type", "Content", "OccurredOn", "RetryCount", "IsPermanentFailure")
                VALUES(@Id, @CorrelationId, @Type, @Content, @OccurredOn, 0, false)
                ON CONFLICT ("CorrelationId") DO NOTHING
                """;

            return unitOfWork.Connection.ExecuteAsync(unitOfWork.CreateCommand(sql, new
            {
                Id = Guid.NewGuid(),
                integrationEvent.CorrelationId,
                Type = integrationEvent.GetType().AssemblyQualifiedName!,
                Content = JsonConvert.SerializeObject(integrationEvent, JsonSerializerSettingsExtensions.Instance),
                integrationEvent.OccurredOn
            }, cancellationToken)).WaitAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<InboxMessage>> GetAsync(int batchSize, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT "Id", "CorrelationId", "Type", "Content", "OccurredOn",
                       "ProcessedOn", "Error", "RetryCount", "NextRetryAt", "IsPermanentFailure"
                FROM users."InboxMessages"
                WHERE "ProcessedOn" IS NULL
                  AND "IsPermanentFailure" = false
                  AND ("NextRetryAt" IS NULL OR "NextRetryAt" <= @Now)
                ORDER BY "OccurredOn"
                LIMIT @BatchSize
                """;

            var result = await unitOfWork.Connection.QueryAsync<InboxMessage>(
                unitOfWork.CreateCommand(sql, new { Now = DateTimeOffset.UtcNow, BatchSize = batchSize }, cancellationToken)).WaitAsync(cancellationToken);

            return result.ToList();
        }

        public Task UpdateAsync(Exception? exception, InboxMessage inboxMessage, CancellationToken cancellationToken)
        {
            const string sql = """
                UPDATE users."InboxMessages"
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

        public Task<bool> IsProcessedAsync(InboxMessageConsumer inboxMessageConsumer, CancellationToken cancellationToken)
        {
            const string sql = """
                SELECT EXISTS (
                    SELECT 1 FROM users."InboxMessageConsumers"
                    WHERE "InboxMessageId" = @InboxMessageId
                      AND "Name" = @Name
                )
                """;

            return unitOfWork.Connection.ExecuteScalarAsync<bool>(
                unitOfWork.CreateCommand(sql, new { inboxMessageConsumer.InboxMessageId, inboxMessageConsumer.Name }, cancellationToken)).WaitAsync(cancellationToken);
        }

        public Task MarkAsProcessedAsync(InboxMessageConsumer inboxMessageConsumer, CancellationToken cancellationToken)
        {
            const string sql = """
                INSERT INTO users."InboxMessageConsumers" ("Id", "InboxMessageId", "Name")
                VALUES (@Id, @InboxMessageId, @Name)
                ON CONFLICT DO NOTHING
                """;

            return unitOfWork.Connection.ExecuteAsync(
                unitOfWork.CreateCommand(sql, new
                {
                    inboxMessageConsumer.Id,
                    inboxMessageConsumer.InboxMessageId,
                    inboxMessageConsumer.Name
                }, cancellationToken)).WaitAsync(cancellationToken);
        }
    }
}
