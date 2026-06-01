using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Catalog.Application.Products.UseCases.Create;
using Modules.Catalog.Application.Products.UseCases.CreateCategory;
using Modules.Catalog.Application.Sellers.UseCases.Create;
using Modules.IntegrationTests.Abstractions;

namespace Modules.IntegrationTests.Catalog.Outbox
{
    public sealed class CatalogOutboxBatchTests(IntegrationWebApplicationFactory factory) : BaseIntegrationTest(factory)
    {
        [Fact]
        public async Task Batch_ProcessesAllMessages_WhenMultipleMessagesPresent()
        {
            // Arrange — create 3 products → 3 outbox messages
            for (var i = 0; i < 3; i++)
                await CreateProductAsync();

            // Act
            await DrainOutboxAsync();

            // Assert
            var messages = await CatalogDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>().ToListAsync();
            messages.Should().HaveCount(3);
            messages.Should().AllSatisfy(m =>
            {
                m.ProcessedOn.Should().NotBeNull();
                m.IsPermanentFailure.Should().BeFalse();
            });

            var consumerCount = await CatalogDbContext.Set<FlashSales.Application.Outbox.OutboxMessageConsumer>().CountAsync();
            consumerCount.Should().Be(3);
        }

        [Fact]
        public async Task DrainOutbox_WhenCancelled_MessageRemainsUnprocessed()
        {
            // Arrange
            await CreateProductAsync();

            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            // Act — pre-cancelled token causes BeginTransactionAsync to throw OperationCanceledException
            var act = () => DrainOutboxAsync(cts.Token);
            await act.Should().ThrowAsync<OperationCanceledException>();

            // Assert — message is untouched
            var outboxMessage = await CatalogDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>().SingleAsync();
            outboxMessage.ProcessedOn.Should().BeNull();
            outboxMessage.RetryCount.Should().Be(0);
            outboxMessage.IsPermanentFailure.Should().BeFalse();
        }

        [Fact]
        public async Task DrainOutbox_AfterTransientInfraFailure_RecoverAndProcessesSuccessfully()
        {
            // Arrange
            await CreateProductAsync();

            using var cts = new CancellationTokenSource();
            await cts.CancelAsync();

            // Simulate infrastructure failure (cancelled drain)
            try { await DrainOutboxAsync(cts.Token); } catch (OperationCanceledException) { }

            // Act — recover and drain normally
            await DrainOutboxAsync();

            // Assert
            var outboxMessage = await CatalogDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>().SingleAsync();
            outboxMessage.ProcessedOn.Should().NotBeNull();
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
