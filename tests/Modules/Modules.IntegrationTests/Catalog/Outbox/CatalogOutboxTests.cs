using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Application.Products.UseCases.Create;
using Modules.Catalog.Application.Products.UseCases.CreateCategory;
using Modules.Catalog.Application.Sellers.UseCases.Create;
using Modules.IntegrationTests.Abstractions;
using Npgsql;

namespace Modules.IntegrationTests.Catalog.Outbox
{
    public sealed class CatalogOutboxTests(IntegrationWebApplicationFactory factory) : BaseIntegrationTest(factory)
    {
        [Fact]
        public async Task DrainOutbox_HappyPath_PublishesIntegrationEventAndMarksAsProcessed()
        {
            // Arrange
            await CreateProductAsync();

            // Act
            await DrainOutboxAsync();

            // Assert
            var outboxMessage = await CatalogDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>()
                .FirstOrDefaultAsync();

            outboxMessage.Should().NotBeNull();
            outboxMessage!.ProcessedOn.Should().NotBeNull();
            outboxMessage.IsPermanentFailure.Should().BeFalse();
            outboxMessage.RetryCount.Should().Be(0);
        }

        [Fact]
        public async Task DrainOutbox_TransientFailure_SchedulesRetry()
        {
            // Arrange
            await CreateProductAsync();

            await using var connection = new NpgsqlConnection(Factory.GetConnectionString());
            await connection.OpenAsync();

            await using var corrupt = new NpgsqlCommand(
                """UPDATE catalog."OutboxMessages" SET "Content" = jsonb_set("Content", ARRAY['$type'], '"NonExistent.Type, NonExistent"') """,
                connection);
            await corrupt.ExecuteNonQueryAsync();

            // Act
            await DrainOutboxAsync();

            // Assert
            var outboxMessage = await CatalogDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>()
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
            await CreateProductAsync();

            await using var connection = new NpgsqlConnection(Factory.GetConnectionString());
            await connection.OpenAsync();

            await using var corrupt = new NpgsqlCommand(
                """UPDATE catalog."OutboxMessages" SET "Content" = jsonb_set("Content", ARRAY['$type'], '"NonExistent.Type, NonExistent"') """,
                connection);
            await corrupt.ExecuteNonQueryAsync();

            // Drain MaxRetryCount (3) times, resetting NextRetryAt between each attempt
            for (var i = 0; i < 3; i++)
            {
                await DrainOutboxAsync();

                await using var resetRetryAt = new NpgsqlCommand(
                    """UPDATE catalog."OutboxMessages" SET "NextRetryAt" = NULL WHERE "IsPermanentFailure" = false""",
                    connection);
                await resetRetryAt.ExecuteNonQueryAsync();
            }

            // Act — final drain causes RetryCount to reach MaxRetryCount
            await DrainOutboxAsync();

            // Assert
            var outboxMessage = await CatalogDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>()
                .FirstOrDefaultAsync();

            outboxMessage.Should().NotBeNull();
            outboxMessage!.IsPermanentFailure.Should().BeTrue();
        }

        [Fact]
        public async Task DrainOutbox_Idempotency_AlreadyProcessedMessageIsNotReprocessed()
        {
            // Arrange
            await CreateProductAsync();
            await DrainOutboxAsync();

            var firstProcessedOn = (await CatalogDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>()
                .FirstAsync()).ProcessedOn;

            // Act
            await DrainOutboxAsync();

            // Assert
            var outboxMessage = await CatalogDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>()
                .FirstAsync();

            outboxMessage.ProcessedOn.Should().Be(firstProcessedOn);
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
