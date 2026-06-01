using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.IntegrationTests.Abstractions;
using Modules.Users.Contracts.IntegrationEvents;

namespace Modules.IntegrationTests.Catalog.Inbox
{
    public sealed class CatalogInboxConcurrencyTests(IntegrationWebApplicationFactory factory) : BaseIntegrationTest(factory)
    {
        [Fact]
        public async Task ConcurrentDrains_CreateSellerExactlyOnce()
        {
            // Arrange
            var integrationEvent = BuildSellerActivatedEvent();
            await InsertCatalogInboxMessageAsync(integrationEvent);

            // Act — two concurrent drains race to process the same inbox message
            await Task.WhenAll(DrainInboxAsync(), DrainInboxAsync());

            // Assert — seller created exactly once
            var sellerCount = await CatalogDbContext.Set<Modules.Catalog.Domain.Sellers.Entities.Seller>().CountAsync();
            sellerCount.Should().Be(1);

            // Consumer record exists exactly once (ON CONFLICT DO NOTHING)
            var consumerCount = await CatalogDbContext.Set<FlashSales.Application.Inbox.InboxMessageConsumer>().CountAsync();
            consumerCount.Should().Be(1);
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
