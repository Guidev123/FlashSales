using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.IntegrationTests.Abstractions;
using Modules.Users.Domain.Users.DomainEvents;

namespace Modules.IntegrationTests.Users.Outbox
{
    public sealed class UsersOutboxTests(IntegrationWebApplicationFactory factory) : BaseIntegrationTest(factory)
    {
        [Fact]
        public async Task DrainOutbox_FlashSalesException_MarksAsPermanentFailure()
        {
            // Arrange — SellerActivatedDomainEvent with a UserId that has no seller profile in DB
            var domainEvent = SellerActivatedDomainEvent.Create(
                userId: Guid.NewGuid(),
                sellerId: Guid.NewGuid());

            await InsertUsersOutboxMessageAsync(domainEvent);

            // Act — handler calls GetSellerProfileAsync(userId) → null → throws FlashSalesException
            await DrainOutboxAsync();

            // Assert
            var outboxMessage = await UsersDbContext.Set<FlashSales.Application.Outbox.OutboxMessage>()
                .SingleAsync();

            outboxMessage.IsPermanentFailure.Should().BeTrue();
            outboxMessage.RetryCount.Should().Be(0);
            outboxMessage.ProcessedOn.Should().BeNull();
            outboxMessage.Error.Should().NotBeNullOrEmpty();
        }
    }
}
