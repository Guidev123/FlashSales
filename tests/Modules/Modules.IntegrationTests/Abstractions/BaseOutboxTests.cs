using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.IntegrationTests.Abstractions;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Modules.IntegrationTests.Abstractions
{
    public abstract class BaseOutboxTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory)
    {
        protected abstract DbContext ModuleDbContext { get; }
        protected abstract string Schema { get; }
        protected abstract Task SeedAsync();

        // ── Happy path ────────────────────────────────────────────────────────

        [Fact]
        public async Task DrainOutbox_HappyPath_PublishesIntegrationEventAndMarksAsProcessed()
        {
            // Arrange
            await SeedAsync();

            // Act
            await DrainOutboxAsync();

            // Assert
            var outboxMessage = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>()
                .FirstOrDefaultAsync();

            outboxMessage.Should().NotBeNull();
            outboxMessage!.ProcessedOn.Should().NotBeNull();
            outboxMessage.IsPermanentFailure.Should().BeFalse();
            outboxMessage.RetryCount.Should().Be(0);
        }

        // ── Retry policy ──────────────────────────────────────────────────────

        [Fact]
        public async Task DrainOutbox_TransientFailure_SchedulesRetry()
        {
            // Arrange
            await SeedAsync();
            await CorruptOutboxTypeAsync();

            // Act
            await DrainOutboxAsync();

            // Assert
            var outboxMessage = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>()
                .FirstOrDefaultAsync();

            outboxMessage.Should().NotBeNull();
            outboxMessage!.ProcessedOn.Should().BeNull();
            outboxMessage.IsPermanentFailure.Should().BeFalse();
            outboxMessage.RetryCount.Should().Be(1);
            outboxMessage.NextRetryAt.Should().NotBeNull();
            outboxMessage.Error.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task DrainOutbox_MaxRetriesExceeded_MarksAsPermanentFailure()
        {
            // Arrange
            await SeedAsync();
            await CorruptOutboxTypeAsync();

            await using var connection = new NpgsqlConnection(Factory.GetConnectionString());
            await connection.OpenAsync();

            for (var i = 0; i < 3; i++)
            {
                await DrainOutboxAsync();

                await using var resetRetryAt = new NpgsqlCommand(
                    $"""UPDATE {Schema}."OutboxMessages" SET "NextRetryAt" = NULL WHERE "IsPermanentFailure" = false""",
                    connection);
                await resetRetryAt.ExecuteNonQueryAsync();
            }

            // Act — final drain reaches MaxRetryCount
            await DrainOutboxAsync();

            // Assert
            var outboxMessage = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>()
                .FirstOrDefaultAsync();

            outboxMessage.Should().NotBeNull();
            outboxMessage!.IsPermanentFailure.Should().BeTrue();
        }

        [Fact]
        public async Task NextRetryAt_BlocksImmediateReprocessing()
        {
            // Arrange
            await SeedAsync();
            await CorruptOutboxTypeAsync();
            await DrainOutboxAsync();

            var afterFirstDrain = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>().SingleAsync();
            afterFirstDrain.RetryCount.Should().Be(1);
            afterFirstDrain.NextRetryAt.Should().NotBeNull();

            // Act — immediate second drain; NextRetryAt is in the future
            await DrainOutboxAsync();

            // Assert — message was not picked up
            var afterSecondDrain = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>().SingleAsync();
            afterSecondDrain.RetryCount.Should().Be(1);
        }

        // ── Idempotency ───────────────────────────────────────────────────────

        [Fact]
        public async Task DrainOutbox_Idempotency_AlreadyProcessedMessageIsNotReprocessed()
        {
            // Arrange
            await SeedAsync();
            await DrainOutboxAsync();

            var firstProcessedOn = (await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>()
                .FirstAsync()).ProcessedOn;

            // Act
            await DrainOutboxAsync();

            // Assert — ProcessedOn unchanged
            var outboxMessage = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>().FirstAsync();
            outboxMessage.ProcessedOn.Should().Be(firstProcessedOn);
        }

        [Fact]
        public async Task OutboxMessageConsumer_HasCorrectIds_AfterSuccessfulDrain()
        {
            // Arrange
            await SeedAsync();

            // Act
            await DrainOutboxAsync();

            // Assert
            var outboxMessage = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>().SingleAsync();
            var consumer = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessageConsumer>().SingleAsync();

            consumer.OutboxMessageId.Should().Be(outboxMessage.Id);
            consumer.OutboxMessageId.Should().NotBe(outboxMessage.CorrelationId);
            consumer.Name.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task OutboxIdempotencyBehavior_SkipsHandler_WhenConsumerRecordExistsFromPreviousCrashedDrain()
        {
            // Arrange — drain successfully; OutboxMessageConsumers row is committed (autocommit)
            await SeedAsync();
            await DrainOutboxAsync();

            var consumersBefore = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessageConsumer>().CountAsync();

            // Simulate crash-after-handler-but-before-outer-commit: reset ProcessedOn
            await using var connection = new NpgsqlConnection(Factory.GetConnectionString());
            await connection.OpenAsync();
            await using var reset = new NpgsqlCommand(
                $"""UPDATE {Schema}."OutboxMessages" SET "ProcessedOn" = NULL""", connection);
            await reset.ExecuteNonQueryAsync();

            // Act — second drain: IsProcessedAsync returns true → handler skipped
            await DrainOutboxAsync();

            // Assert
            var outboxMessage = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>().SingleAsync();
            outboxMessage.ProcessedOn.Should().NotBeNull();

            var consumersAfter = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessageConsumer>().CountAsync();
            consumersAfter.Should().Be(consumersBefore);
        }

        // ── Batch ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Batch_ProcessesAllMessages_WhenMultipleMessagesPresent()
        {
            // Arrange
            for (var i = 0; i < 3; i++)
                await SeedAsync();

            // Act
            await DrainOutboxAsync();

            // Assert
            var messages = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>().ToListAsync();
            messages.Should().HaveCount(3);
            messages.Should().AllSatisfy(m =>
            {
                m.ProcessedOn.Should().NotBeNull();
                m.IsPermanentFailure.Should().BeFalse();
            });

            var consumerCount = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessageConsumer>().CountAsync();
            consumerCount.Should().Be(3);
        }

        // ── Cancellation / infra failure ──────────────────────────────────────

        [Fact]
        public async Task DrainOutbox_WhenCancelled_MessageRemainsUnprocessed()
        {
            // Arrange
            await SeedAsync();

            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            // Act
            var act = () => DrainOutboxAsync(cts.Token);
            await act.Should().ThrowAsync<OperationCanceledException>();

            // Assert
            var outboxMessage = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>().SingleAsync();
            outboxMessage.ProcessedOn.Should().BeNull();
            outboxMessage.RetryCount.Should().Be(0);
            outboxMessage.IsPermanentFailure.Should().BeFalse();
        }

        [Fact]
        public async Task DrainOutbox_AfterTransientInfraFailure_RecoverAndProcessesSuccessfully()
        {
            // Arrange
            await SeedAsync();

            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            try { await DrainOutboxAsync(cts.Token); } catch (OperationCanceledException) { }

            // Act
            await DrainOutboxAsync();

            // Assert
            var outboxMessage = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>().SingleAsync();
            outboxMessage.ProcessedOn.Should().NotBeNull();
        }

        // ── Concurrency ───────────────────────────────────────────────────────

        [Fact]
        public async Task ConcurrentDrains_ProduceExactlyOneConsumerRecord()
        {
            // Arrange
            await SeedAsync();

            // Act — two concurrent drains race to process the same message
            await Task.WhenAll(DrainOutboxAsync(), DrainOutboxAsync());

            // Assert
            var consumerCount = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessageConsumer>().CountAsync();
            consumerCount.Should().Be(1);

            var outboxMessage = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>().SingleAsync();
            outboxMessage.ProcessedOn.Should().NotBeNull();
        }

        // ── Correlation ID flow ───────────────────────────────────────────────

        [Fact]
        public async Task OutboxMessage_CorrelationId_MatchesDomainEventCorrelationId_And_IdIsDifferent()
        {
            // Arrange
            await SeedAsync();

            // Assert
            var outboxMessage = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>().SingleAsync();
            var content = JObject.Parse(outboxMessage.Content);
            var correlationIdInJson = Guid.Parse(content["CorrelationId"]!.ToString());

            outboxMessage.CorrelationId.Should().Be(correlationIdInJson);
            outboxMessage.Id.Should().NotBe(outboxMessage.CorrelationId);
        }

        [Fact]
        public async Task OutboxMessageConsumer_OutboxMessageId_IsOutboxMessageId_NotCorrelationId()
        {
            // Arrange
            await SeedAsync();

            // Act
            await DrainOutboxAsync();

            // Assert
            var outboxMessage = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>().SingleAsync();
            var consumer = await ModuleDbContext.Set<FlashSales.Application.Outbox.OutboxMessageConsumer>().SingleAsync();

            consumer.OutboxMessageId.Should().Be(outboxMessage.Id);
            consumer.OutboxMessageId.Should().NotBe(outboxMessage.CorrelationId);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async Task CorruptOutboxTypeAsync()
        {
            await using var connection = new NpgsqlConnection(Factory.GetConnectionString());
            await connection.OpenAsync();
            await using var corrupt = new NpgsqlCommand(
                $"""UPDATE {Schema}."OutboxMessages" SET "Content" = jsonb_set("Content", ARRAY['$type'], '"NonExistent.Type, NonExistent"') """,
                connection);
            await corrupt.ExecuteNonQueryAsync();
        }
    }
}
