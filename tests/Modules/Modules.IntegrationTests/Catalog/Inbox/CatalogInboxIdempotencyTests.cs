using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.IntegrationTests.Abstractions;
using Modules.Users.Contracts.IntegrationEvents;
using Npgsql;

namespace Modules.IntegrationTests.Catalog.Inbox
{
    public sealed class CatalogInboxIdempotencyTests(IntegrationWebApplicationFactory factory) : BaseIntegrationTest(factory)
    {
        [Fact]
        public async Task InboxMessageConsumer_HasCorrectIds_AfterSuccessfulDrain()
        {
            // Arrange
            var integrationEvent = BuildSellerActivatedEvent();
            await InsertCatalogInboxMessageAsync(integrationEvent);

            // Act
            await DrainInboxAsync();

            // Assert
            var inboxMessage = await CatalogDbContext.Set<FlashSales.Application.Inbox.InboxMessage>()
                .SingleAsync();

            var consumer = await CatalogDbContext.Set<FlashSales.Application.Inbox.InboxMessageConsumer>()
                .SingleAsync();

            consumer.InboxMessageId.Should().Be(inboxMessage.Id);
            consumer.InboxMessageId.Should().NotBe(inboxMessage.CorrelationId);
            consumer.Name.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task InboxIdempotencyBehavior_SkipsHandler_WhenConsumerRecordExistsFromPreviousCrashedDrain()
        {
            // Arrange — drain successfully so InboxMessageConsumers row is committed (autocommit)
            var integrationEvent = BuildSellerActivatedEvent();
            await InsertCatalogInboxMessageAsync(integrationEvent);
            await DrainInboxAsync();

            var sellerCountBefore = await CatalogDbContext.Set<Modules.Catalog.Domain.Sellers.Entities.Seller>().CountAsync();
            var consumersBefore = await CatalogDbContext.Set<FlashSales.Application.Inbox.InboxMessageConsumer>().CountAsync();

            // Simulate crash-after-handler-but-before-outer-commit: reset ProcessedOn
            await using var connection = new NpgsqlConnection(Factory.GetConnectionString());
            await connection.OpenAsync();
            await using var reset = new NpgsqlCommand(
                """UPDATE catalog."InboxMessages" SET "ProcessedOn" = NULL""", connection);
            await reset.ExecuteNonQueryAsync();

            // Act — second drain: IsProcessedAsync returns true → handler skipped → no duplicate seller
            await DrainInboxAsync();

            // Assert
            var inboxMessage = await CatalogDbContext.Set<FlashSales.Application.Inbox.InboxMessage>().SingleAsync();
            inboxMessage.ProcessedOn.Should().NotBeNull();

            var sellerCountAfter = await CatalogDbContext.Set<Modules.Catalog.Domain.Sellers.Entities.Seller>().CountAsync();
            sellerCountAfter.Should().Be(sellerCountBefore);

            var consumersAfter = await CatalogDbContext.Set<FlashSales.Application.Inbox.InboxMessageConsumer>().CountAsync();
            consumersAfter.Should().Be(consumersBefore);
        }

        [Fact]
        public async Task DrainInbox_WhenCancelled_MessageRemainsUnprocessed()
        {
            // Arrange
            var integrationEvent = BuildSellerActivatedEvent();
            await InsertCatalogInboxMessageAsync(integrationEvent);

            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            // Act
            var act = () => DrainInboxAsync(cts.Token);
            await act.Should().ThrowAsync<OperationCanceledException>();

            // Assert
            var inboxMessage = await CatalogDbContext.Set<FlashSales.Application.Inbox.InboxMessage>().SingleAsync();
            inboxMessage.ProcessedOn.Should().BeNull();
            inboxMessage.RetryCount.Should().Be(0);
        }

        [Fact]
        public async Task NextRetryAt_BlocksImmediateReprocessing()
        {
            // Arrange — corrupt type to force transient failure
            var integrationEvent = BuildSellerActivatedEvent();
            await InsertCatalogInboxMessageAsync(integrationEvent);

            await using var connection = new NpgsqlConnection(Factory.GetConnectionString());
            await connection.OpenAsync();
            await using var corrupt = new NpgsqlCommand(
                """UPDATE catalog."InboxMessages" SET "Content" = jsonb_set("Content", ARRAY['$type'], '"NonExistent.Type, NonExistent"') """,
                connection);
            await corrupt.ExecuteNonQueryAsync();

            await DrainInboxAsync();

            var afterFirstDrain = await CatalogDbContext.Set<FlashSales.Application.Inbox.InboxMessage>().SingleAsync();
            afterFirstDrain.RetryCount.Should().Be(1);
            afterFirstDrain.NextRetryAt.Should().NotBeNull();

            // Act — immediate second drain
            await DrainInboxAsync();

            // Assert — NextRetryAt in the future prevents pickup
            var afterSecondDrain = await CatalogDbContext.Set<FlashSales.Application.Inbox.InboxMessage>().SingleAsync();
            afterSecondDrain.RetryCount.Should().Be(1);
        }

        private static SellerActivatedIntegrationEvent BuildSellerActivatedEvent()
            => SellerActivatedIntegrationEvent.Create(
                correlationId: Guid.NewGuid(),
                userId: Guid.NewGuid(),
                sellerId: Guid.NewGuid(),
                name: "Test Seller",
                profilePictureUrl: null,
                isActive: true);
    }
}
