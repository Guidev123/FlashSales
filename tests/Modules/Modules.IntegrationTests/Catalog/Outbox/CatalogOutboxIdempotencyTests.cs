using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Application.Products.UseCases.Create;
using Modules.Catalog.Application.Products.UseCases.CreateCategory;
using Modules.Catalog.Application.Sellers.UseCases.Create;
using Modules.IntegrationTests.Abstractions;
using Npgsql;

namespace Modules.IntegrationTests.Catalog.Outbox
{
    public sealed class CatalogOutboxIdempotencyTests(IntegrationWebApplicationFactory factory) : BaseIntegrationTest(factory)
    {
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

        [Fact]
        public async Task OutboxMessageConsumer_HasCorrectIds_AfterSuccessfulDrain()
        {
            // Arrange
            await CreateProductAsync();

            // Act
            await DrainOutboxAsync();

            // Assert
            var outboxMessage = await CatalogDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>()
                .SingleAsync();

            var consumer = await CatalogDbContext.Set<FlashSales.Application.Outbox.OutboxMessageConsumer>()
                .SingleAsync();

            consumer.OutboxMessageId.Should().Be(outboxMessage.Id);
            consumer.OutboxMessageId.Should().NotBe(outboxMessage.CorrelationId);
            consumer.Name.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task OutboxIdempotencyBehavior_SkipsHandler_WhenConsumerRecordExistsFromPreviousCrashedDrain()
        {
            // Arrange — drain successfully so OutboxMessageConsumers row is committed (autocommit)
            await CreateProductAsync();
            await DrainOutboxAsync();

            var consumersBefore = await CatalogDbContext.Set<FlashSales.Application.Outbox.OutboxMessageConsumer>().CountAsync();

            // Simulate crash-after-handler-but-before-outer-commit: reset ProcessedOn
            await using var connection = new NpgsqlConnection(Factory.GetConnectionString());
            await connection.OpenAsync();
            await using var reset = new NpgsqlCommand(
                """UPDATE catalog."OutboxMessages" SET "ProcessedOn" = NULL""", connection);
            await reset.ExecuteNonQueryAsync();

            // Act — second drain: IsProcessedAsync returns true → handler skipped → ProcessedOn set
            await DrainOutboxAsync();

            // Assert
            var outboxMessage = await CatalogDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>()
                .SingleAsync();

            outboxMessage.ProcessedOn.Should().NotBeNull();

            var consumersAfter = await CatalogDbContext.Set<FlashSales.Application.Outbox.OutboxMessageConsumer>().CountAsync();
            consumersAfter.Should().Be(consumersBefore);
        }
    }
}
