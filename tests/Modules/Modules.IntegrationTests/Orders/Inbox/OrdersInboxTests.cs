using FluentAssertions;
using FlashSales.Application.Messaging;
using Microsoft.EntityFrameworkCore;
using Modules.IntegrationTests.Abstractions;
using Modules.Launches.Application.Launches.Features.Activate;
using Modules.Launches.Application.Launches.Features.Create;
using Modules.Launches.Application.Launches.Features.Schedule;
using Modules.Launches.Application.Sellers.Features.Create;
using Modules.Launches.Contracts.IntegrationEvents;
using Modules.Orders.Application.Orders.Features.Create;
using Modules.Orders.Domain.Launches.Enums;
using Modules.Orders.Domain.Orders.Enums;
using Modules.Payments.Contracts.IntegrationEvents;

namespace Modules.IntegrationTests.Orders.Inbox
{
    public sealed class OrdersInboxTests(IntegrationWebApplicationFactory factory)
        : BaseInboxTests(factory)
    {
        protected override DbContext ModuleDbContext => OrdersDbContext;
        protected override string Schema => "orders";

        protected override IntegrationEvent BuildEvent()
            => BuildLaunchActivatedEvent();

        protected override Task InsertInboxMessageAsync(IntegrationEvent evt, CancellationToken cancellationToken = default)
            => InsertOrdersInboxMessageAsync(evt, cancellationToken);

        [Fact]
        public async Task DrainInbox_LaunchActivated_CreatesLocalLaunchReplica()
        {
            // Arrange
            var integrationEvent = BuildLaunchActivatedEvent();
            await InsertOrdersInboxMessageAsync(integrationEvent);

            // Act
            await DrainInboxAsync();

            // Assert
            var launch = await OrdersDbContext.Set<Modules.Orders.Domain.Launches.Entities.Launch>()
                .FirstOrDefaultAsync(l => l.Id == integrationEvent.LaunchId);

            launch.Should().NotBeNull();
            launch!.Status.Should().Be(LaunchStatus.Active);
        }

        [Fact]
        public async Task DrainInbox_PaymentCompleted_ConfirmsCorrespondingOrder()
        {
            // Arrange
            var orderId = await CreateAwaitingPaymentOrderAsync();
            var integrationEvent = PaymentCompletedIntegrationEvent.Create(Guid.NewGuid(), Guid.NewGuid(), orderId, 100m);
            await InsertOrdersInboxMessageAsync(integrationEvent);

            // Act
            await DrainInboxAsync();

            // Assert
            var order = await OrdersDbContext.Set<Modules.Orders.Domain.Orders.Entities.Order>()
                .AsNoTracking()
                .FirstAsync(o => o.Id == orderId);

            order.Status.Should().Be(OrderStatus.Confirmed);
        }

        [Fact]
        public async Task DrainInbox_PaymentFailed_CancelsCorrespondingOrder()
        {
            // Arrange
            var orderId = await CreateAwaitingPaymentOrderAsync();
            var integrationEvent = PaymentFailedIntegrationEvent.Create(Guid.NewGuid(), Guid.NewGuid(), orderId, 100m, "Card declined");
            await InsertOrdersInboxMessageAsync(integrationEvent);

            // Act
            await DrainInboxAsync();

            // Assert
            var order = await OrdersDbContext.Set<Modules.Orders.Domain.Orders.Entities.Order>()
                .AsNoTracking()
                .FirstAsync(o => o.Id == orderId);

            order.Status.Should().Be(OrderStatus.Cancelled);
        }

        private static LaunchActivatedIntegrationEvent BuildLaunchActivatedEvent()
            => LaunchActivatedIntegrationEvent.Create(
                correlationId: Guid.NewGuid(),
                launchId: Guid.NewGuid(),
                sellerId: Guid.NewGuid(),
                productId: Guid.NewGuid(),
                title: "Test Launch",
                discountedPrice: 50m,
                originalPrice: 100m,
                totalQuantity: 10,
                startAt: DateTimeOffset.UtcNow.AddMinutes(5),
                endAt: DateTimeOffset.UtcNow.AddHours(2));

        private async Task<Guid> CreateAwaitingPaymentOrderAsync()
        {
            var userId = Guid.NewGuid();
            var sellerId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var title = Faker.Commerce.ProductName();

            await Mediator.SendAsync(new CreateSellerCommand(userId, sellerId, Faker.Company.CompanyName(), null, true));

            var createLaunch = await Mediator.SendAsync(new CreateLaunchCommand(
                userId, productId, title, Faker.Commerce.ProductDescription()));
            var launchId = createLaunch.Value.Id;

            var startAt = DateTimeOffset.UtcNow.AddMinutes(5);
            var endAt = DateTimeOffset.UtcNow.AddHours(2);

            await Mediator.SendAsync(new ScheduleLaunchCommand(userId, launchId, 50m, 100m, 10, 0, startAt, endAt));
            await Mediator.SendAsync(new ActivateLaunchCommand(launchId));

            await Mediator.SendAsync(new Modules.Orders.Application.Launches.Features.Create.CreateLaunchCommand(
                launchId, sellerId, productId, title, 50m, 100m, 10, startAt, endAt));

            var createOrder = await Mediator.SendAsync(new CreateOrderCommand(
                Guid.NewGuid(), Faker.Internet.Email(), launchId, 1));

            return createOrder.Value.OrderId;
        }
    }
}
