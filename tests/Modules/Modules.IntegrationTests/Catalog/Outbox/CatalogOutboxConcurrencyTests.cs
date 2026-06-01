using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Application.Products.UseCases.Create;
using Modules.Catalog.Application.Products.UseCases.CreateCategory;
using Modules.Catalog.Application.Sellers.UseCases.Create;
using Modules.IntegrationTests.Abstractions;
using Npgsql;

namespace Modules.IntegrationTests.Catalog.Outbox
{
    public sealed class CatalogOutboxConcurrencyTests(IntegrationWebApplicationFactory factory) : BaseIntegrationTest(factory)
    {
        [Fact]
        public async Task ConcurrentDrains_ProduceExactlyOneConsumerRecord()
        {
            // Arrange
            await CreateProductAsync();

            // Act — two concurrent drains race to process the same message
            await Task.WhenAll(DrainOutboxAsync(), DrainOutboxAsync());

            // Assert — idempotency tables (ON CONFLICT DO NOTHING) guarantee exactly one consumer record
            var consumerCount = await CatalogDbContext.Set<FlashSales.Application.Outbox.OutboxMessageConsumer>().CountAsync();
            consumerCount.Should().Be(1);

            var outboxMessage = await CatalogDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>().SingleAsync();
            outboxMessage.ProcessedOn.Should().NotBeNull();
        }

        [Fact]
        public async Task NextRetryAt_BlocksImmediateReprocessing()
        {
            // Arrange — first drain fails with transient error
            await CreateProductAsync();

            await using var connection = new NpgsqlConnection(Factory.GetConnectionString());
            await connection.OpenAsync();
            await using var corrupt = new NpgsqlCommand(
                """UPDATE catalog."OutboxMessages" SET "Content" = jsonb_set("Content", ARRAY['$type'], '"NonExistent.Type, NonExistent"') """,
                connection);
            await corrupt.ExecuteNonQueryAsync();

            await DrainOutboxAsync();

            var afterFirstDrain = await CatalogDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>().SingleAsync();
            afterFirstDrain.RetryCount.Should().Be(1);
            afterFirstDrain.NextRetryAt.Should().NotBeNull();

            // Act — immediate second drain with NextRetryAt in the future
            await DrainOutboxAsync();

            // Assert — message was NOT picked up by second drain
            var afterSecondDrain = await CatalogDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>().SingleAsync();
            afterSecondDrain.RetryCount.Should().Be(1);
        }

        private async Task CreateProductAsync()
        {
            var userId = Guid.NewGuid();

            await Mediator.SendAsync(new CreateSellerCommand(
                UserId: userId,
                SellerId: Guid.NewGuid(),
                Name: Faker.Company.CompanyName(),
                ProfilePictureUrl: null,
                IsActive: true));

            var category = await Mediator.SendAsync(
                new CreateCategoryCommand(Faker.Commerce.Categories(1)[0] + Guid.NewGuid()));

            await Mediator.SendAsync(new CreateProductCommand(
                UserId: userId,
                CategoryId: category.Value.Id,
                Name: Faker.Commerce.ProductName(),
                Description: Faker.Commerce.ProductDescription()));
        }
    }
}
