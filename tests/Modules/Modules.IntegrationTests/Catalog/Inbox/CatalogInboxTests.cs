using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.IntegrationTests.Abstractions;
using Modules.Users.Contracts.IntegrationEvents;
using Npgsql;

namespace Modules.IntegrationTests.Catalog.Inbox
{
    public sealed class CatalogInboxTests(IntegrationWebApplicationFactory factory) : BaseIntegrationTest(factory)
    {
        [Fact]
        public async Task DrainInbox_HappyPath_CreatesSellerAndMarksMessageAsProcessed()
        {
            // Arrange
            var integrationEvent = BuildSellerActivatedEvent();
            await InsertCatalogInboxMessageAsync(integrationEvent);

            // Act
            await DrainInboxAsync();

            // Assert
            var seller = await CatalogDbContext.Set<Modules.Catalog.Domain.Sellers.Entities.Seller>()
                .FirstOrDefaultAsync(s => s.SellerId == integrationEvent.SellerId);

            seller.Should().NotBeNull();

            var inboxMessage = await CatalogDbContext.Set<FlashSales.Application.Inbox.InboxMessage>()
                .FirstOrDefaultAsync();

            inboxMessage.Should().NotBeNull();
            inboxMessage!.ProcessedOn.Should().NotBeNull();
            inboxMessage.IsPermanentFailure.Should().BeFalse();
        }

        [Fact]
        public async Task InsertCatalogInboxMessage_DuplicateCorrelationId_OnlyOneMessageInserted()
        {
            // Arrange
            var integrationEvent = BuildSellerActivatedEvent();

            // Act
            await InsertCatalogInboxMessageAsync(integrationEvent);
            await InsertCatalogInboxMessageAsync(integrationEvent);

            // Assert
            var count = await CatalogDbContext.Set<FlashSales.Application.Inbox.InboxMessage>().CountAsync();
            count.Should().Be(1);
        }

        [Fact]
        public async Task DrainInbox_DuplicateSeller_MarksAsPermanentFailure()
        {
            // Arrange — process first event normally so the seller already exists
            var firstEvent = BuildSellerActivatedEvent();
            await InsertCatalogInboxMessageAsync(firstEvent);
            await DrainInboxAsync();

            // Second event with same SellerId + UserId but different CorrelationId triggers FlashSalesException
            var duplicateEvent = BuildSellerActivatedEvent(
                sellerId: firstEvent.SellerId,
                userId: firstEvent.UserId);

            await InsertCatalogInboxMessageAsync(duplicateEvent);

            // Act
            await DrainInboxAsync();

            // Assert
            var failedMessage = await CatalogDbContext.Set<FlashSales.Application.Inbox.InboxMessage>()
                .FirstOrDefaultAsync(m => m.IsPermanentFailure);

            failedMessage.Should().NotBeNull();
            failedMessage!.ProcessedOn.Should().BeNull();
            failedMessage.RetryCount.Should().Be(0);
        }

        [Fact]
        public async Task DrainInbox_Idempotency_AlreadyProcessedMessageIsNotReprocessed()
        {
            // Arrange
            var integrationEvent = BuildSellerActivatedEvent();
            await InsertCatalogInboxMessageAsync(integrationEvent);
            await DrainInboxAsync();

            var sellerCountBefore = await CatalogDbContext.Set<Modules.Catalog.Domain.Sellers.Entities.Seller>().CountAsync();

            // Act
            await DrainInboxAsync();

            // Assert
            var sellerCountAfter = await CatalogDbContext.Set<Modules.Catalog.Domain.Sellers.Entities.Seller>().CountAsync();
            sellerCountAfter.Should().Be(sellerCountBefore);
        }

        [Fact]
        public async Task DrainInbox_TransientFailure_SchedulesRetry()
        {
            // Arrange
            var integrationEvent = BuildSellerActivatedEvent();
            await InsertCatalogInboxMessageAsync(integrationEvent);

            await using var connection = new NpgsqlConnection(Factory.GetConnectionString());
            await connection.OpenAsync();

            await using var corrupt = new NpgsqlCommand(
                """UPDATE catalog."InboxMessages" SET "Content" = jsonb_set("Content", ARRAY['$type'], '"NonExistent.Type, NonExistent"') """,
                connection);
            await corrupt.ExecuteNonQueryAsync();

            // Act
            await DrainInboxAsync();

            // Assert
            var inboxMessage = await CatalogDbContext.Set<FlashSales.Application.Inbox.InboxMessage>()
                .FirstOrDefaultAsync();

            inboxMessage.Should().NotBeNull();
            inboxMessage!.ProcessedOn.Should().BeNull();
            inboxMessage.IsPermanentFailure.Should().BeFalse();
            inboxMessage.RetryCount.Should().Be(1);
            inboxMessage.NextRetryAt.Should().NotBeNull();
        }

        private static SellerActivatedIntegrationEvent BuildSellerActivatedEvent(
            Guid? sellerId = null,
            Guid? userId = null)
        {
            return SellerActivatedIntegrationEvent.Create(
                correlationId: Guid.NewGuid(),
                userId: userId ?? Guid.NewGuid(),
                sellerId: sellerId ?? Guid.NewGuid(),
                name: "Test Seller",
                profilePictureUrl: null,
                isActive: true);
        }
    }
}
