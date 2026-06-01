using FluentAssertions;
using FlashSales.Application.Messaging;
using Microsoft.EntityFrameworkCore;
using Modules.IntegrationTests.Abstractions;
using Npgsql;

namespace Modules.IntegrationTests.Abstractions
{
    public abstract class BaseInboxTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory)
    {
        protected abstract DbContext ModuleDbContext { get; }
        protected abstract string Schema { get; }
        protected abstract IntegrationEvent BuildEvent();
        protected abstract Task InsertInboxMessageAsync(IntegrationEvent evt, CancellationToken cancellationToken = default);

        private Task SeedAsync() => InsertInboxMessageAsync(BuildEvent());

        // ── Happy path ────────────────────────────────────────────────────────

        [Fact]
        public async Task DrainInbox_HappyPath_MarksMessageAsProcessed()
        {
            // Arrange
            await SeedAsync();

            // Act
            await DrainInboxAsync();

            // Assert
            var inboxMessage = await ModuleDbContext.Set<FlashSales.Application.Inbox.InboxMessage>()
                .FirstOrDefaultAsync();

            inboxMessage.Should().NotBeNull();
            inboxMessage!.ProcessedOn.Should().NotBeNull();
            inboxMessage.IsPermanentFailure.Should().BeFalse();
        }

        // ── Deduplication ─────────────────────────────────────────────────────

        [Fact]
        public async Task InsertInboxMessage_DuplicateCorrelationId_OnlyOneMessageInserted()
        {
            // Arrange
            var integrationEvent = BuildEvent();

            // Act
            await InsertInboxMessageAsync(integrationEvent);
            await InsertInboxMessageAsync(integrationEvent);

            // Assert
            var count = await ModuleDbContext.Set<FlashSales.Application.Inbox.InboxMessage>().CountAsync();
            count.Should().Be(1);
        }

        // ── Retry policy ──────────────────────────────────────────────────────

        [Fact]
        public async Task DrainInbox_TransientFailure_SchedulesRetry()
        {
            // Arrange
            await SeedAsync();
            await CorruptInboxTypeAsync();

            // Act
            await DrainInboxAsync();

            // Assert
            var inboxMessage = await ModuleDbContext.Set<FlashSales.Application.Inbox.InboxMessage>()
                .FirstOrDefaultAsync();

            inboxMessage.Should().NotBeNull();
            inboxMessage!.ProcessedOn.Should().BeNull();
            inboxMessage.IsPermanentFailure.Should().BeFalse();
            inboxMessage.RetryCount.Should().Be(1);
            inboxMessage.NextRetryAt.Should().NotBeNull();
        }

        [Fact]
        public async Task NextRetryAt_BlocksImmediateReprocessing()
        {
            // Arrange
            await SeedAsync();
            await CorruptInboxTypeAsync();
            await DrainInboxAsync();

            var afterFirstDrain = await ModuleDbContext.Set<FlashSales.Application.Inbox.InboxMessage>().SingleAsync();
            afterFirstDrain.RetryCount.Should().Be(1);
            afterFirstDrain.NextRetryAt.Should().NotBeNull();

            // Act — immediate second drain; NextRetryAt is in the future
            await DrainInboxAsync();

            // Assert — message was not picked up
            var afterSecondDrain = await ModuleDbContext.Set<FlashSales.Application.Inbox.InboxMessage>().SingleAsync();
            afterSecondDrain.RetryCount.Should().Be(1);
        }

        // ── Idempotency ───────────────────────────────────────────────────────

        [Fact]
        public async Task InboxMessageConsumer_HasCorrectIds_AfterSuccessfulDrain()
        {
            // Arrange
            await SeedAsync();

            // Act
            await DrainInboxAsync();

            // Assert
            var inboxMessage = await ModuleDbContext.Set<FlashSales.Application.Inbox.InboxMessage>().SingleAsync();
            var consumer = await ModuleDbContext.Set<FlashSales.Application.Inbox.InboxMessageConsumer>().SingleAsync();

            consumer.InboxMessageId.Should().Be(inboxMessage.Id);
            consumer.InboxMessageId.Should().NotBe(inboxMessage.CorrelationId);
            consumer.Name.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task DrainInbox_Idempotency_AlreadyProcessedMessageIsNotReprocessed()
        {
            // Arrange
            await SeedAsync();
            await DrainInboxAsync();

            var consumersBefore = await ModuleDbContext.Set<FlashSales.Application.Inbox.InboxMessageConsumer>().CountAsync();

            // Act
            await DrainInboxAsync();

            // Assert — no new consumer records created
            var consumersAfter = await ModuleDbContext.Set<FlashSales.Application.Inbox.InboxMessageConsumer>().CountAsync();
            consumersAfter.Should().Be(consumersBefore);
        }

        [Fact]
        public async Task InboxIdempotencyBehavior_SkipsHandler_WhenConsumerRecordExistsFromPreviousCrashedDrain()
        {
            // Arrange — drain successfully; InboxMessageConsumers row is committed (autocommit)
            await SeedAsync();
            await DrainInboxAsync();

            var consumersBefore = await ModuleDbContext.Set<FlashSales.Application.Inbox.InboxMessageConsumer>().CountAsync();

            // Simulate crash-after-handler-but-before-outer-commit: reset ProcessedOn
            await using var connection = new NpgsqlConnection(Factory.GetConnectionString());
            await connection.OpenAsync();
            await using var reset = new NpgsqlCommand(
                $"""UPDATE {Schema}."InboxMessages" SET "ProcessedOn" = NULL""", connection);
            await reset.ExecuteNonQueryAsync();

            // Act — second drain: IsProcessedAsync returns true → handler skipped
            await DrainInboxAsync();

            // Assert
            var inboxMessage = await ModuleDbContext.Set<FlashSales.Application.Inbox.InboxMessage>().SingleAsync();
            inboxMessage.ProcessedOn.Should().NotBeNull();

            var consumersAfter = await ModuleDbContext.Set<FlashSales.Application.Inbox.InboxMessageConsumer>().CountAsync();
            consumersAfter.Should().Be(consumersBefore);
        }

        // ── Cancellation ──────────────────────────────────────────────────────

        [Fact]
        public async Task DrainInbox_WhenCancelled_MessageRemainsUnprocessed()
        {
            // Arrange
            await SeedAsync();

            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            // Act
            var act = () => DrainInboxAsync(cts.Token);
            await act.Should().ThrowAsync<OperationCanceledException>();

            // Assert
            var inboxMessage = await ModuleDbContext.Set<FlashSales.Application.Inbox.InboxMessage>().SingleAsync();
            inboxMessage.ProcessedOn.Should().BeNull();
            inboxMessage.RetryCount.Should().Be(0);
        }

        // ── Concurrency ───────────────────────────────────────────────────────

        [Fact]
        public async Task ConcurrentDrains_ProduceExactlyOneConsumerRecord()
        {
            // Arrange
            await SeedAsync();

            // Act — two concurrent drains race to process the same message
            await Task.WhenAll(DrainInboxAsync(), DrainInboxAsync());

            // Assert — consumer record created exactly once
            var consumerCount = await ModuleDbContext.Set<FlashSales.Application.Inbox.InboxMessageConsumer>().CountAsync();
            consumerCount.Should().Be(1);
        }

        // ── Correlation ID flow ───────────────────────────────────────────────

        [Fact]
        public async Task InboxMessage_CorrelationId_MatchesIntegrationEventCorrelationId_And_IdIsDifferent()
        {
            // Arrange
            var integrationEvent = BuildEvent();
            await InsertInboxMessageAsync(integrationEvent);

            // Assert
            var inboxMessage = await ModuleDbContext.Set<FlashSales.Application.Inbox.InboxMessage>().SingleAsync();

            inboxMessage.CorrelationId.Should().Be(integrationEvent.CorrelationId);
            inboxMessage.Id.Should().NotBe(inboxMessage.CorrelationId);
        }

        [Fact]
        public async Task InboxMessageConsumer_InboxMessageId_IsInboxMessageId_NotCorrelationId()
        {
            // Arrange
            await SeedAsync();

            // Act
            await DrainInboxAsync();

            // Assert
            var inboxMessage = await ModuleDbContext.Set<FlashSales.Application.Inbox.InboxMessage>().SingleAsync();
            var consumer = await ModuleDbContext.Set<FlashSales.Application.Inbox.InboxMessageConsumer>().SingleAsync();

            consumer.InboxMessageId.Should().Be(inboxMessage.Id);
            consumer.InboxMessageId.Should().NotBe(inboxMessage.CorrelationId);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private async Task CorruptInboxTypeAsync()
        {
            await using var connection = new NpgsqlConnection(Factory.GetConnectionString());
            await connection.OpenAsync();
            await using var corrupt = new NpgsqlCommand(
                $"""UPDATE {Schema}."InboxMessages" SET "Content" = jsonb_set("Content", ARRAY['$type'], '"NonExistent.Type, NonExistent"') """,
                connection);
            await corrupt.ExecuteNonQueryAsync();
        }
    }
}
