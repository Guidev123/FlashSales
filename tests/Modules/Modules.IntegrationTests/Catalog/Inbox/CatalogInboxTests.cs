using FluentAssertions;
using FlashSales.Application.Messaging;
using Microsoft.EntityFrameworkCore;
using Modules.IntegrationTests.Abstractions;
using Modules.Users.Contracts.IntegrationEvents;

namespace Modules.IntegrationTests.Catalog.Inbox
{
    public sealed class CatalogInboxTests(IntegrationWebApplicationFactory factory)
        : BaseInboxTests(factory)
    {
        protected override DbContext ModuleDbContext => CatalogDbContext;
        protected override string Schema => "catalog";

        protected override IntegrationEvent BuildEvent()
            => BuildSellerActivatedEvent();

        protected override Task InsertInboxMessageAsync(IntegrationEvent evt, CancellationToken cancellationToken = default)
            => InsertCatalogInboxMessageAsync(evt, cancellationToken);

        // ── Catalog-specific ─────────────────────────────────────────────────

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
                .FirstOrDefaultAsync(s => s.Id == integrationEvent.SellerId);

            seller.Should().NotBeNull();

            var inboxMessage = await CatalogDbContext.Set<FlashSales.Application.Inbox.InboxMessage>()
                .FirstOrDefaultAsync();

            inboxMessage.Should().NotBeNull();
            inboxMessage!.ProcessedOn.Should().NotBeNull();
            inboxMessage.IsPermanentFailure.Should().BeFalse();
        }

        [Fact]
        public async Task DrainInbox_DuplicateSeller_MarksAsPermanentFailure()
        {
            // Arrange — process first event so seller already exists
            var firstEvent = BuildSellerActivatedEvent();
            await InsertCatalogInboxMessageAsync(firstEvent);
            await DrainInboxAsync();

            // Second event with same SellerId triggers FlashSalesException
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
        public async Task DrainInbox_Idempotency_DoesNotCreateDuplicateSeller()
        {
            // Arrange
            await InsertCatalogInboxMessageAsync(BuildEvent());
            await DrainInboxAsync();

            var sellerCountBefore = await CatalogDbContext.Set<Modules.Catalog.Domain.Sellers.Entities.Seller>().CountAsync();

            // Act
            await DrainInboxAsync();

            // Assert
            var sellerCountAfter = await CatalogDbContext.Set<Modules.Catalog.Domain.Sellers.Entities.Seller>().CountAsync();
            sellerCountAfter.Should().Be(sellerCountBefore);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static SellerActivatedIntegrationEvent BuildSellerActivatedEvent(
            Guid? sellerId = null,
            Guid? userId = null)
            => SellerActivatedIntegrationEvent.Create(
                correlationId: Guid.NewGuid(),
                userId: userId ?? Guid.NewGuid(),
                sellerId: sellerId ?? Guid.NewGuid(),
                name: "Test Seller",
                profilePictureUrl: null,
                isActive: true);
    }
}