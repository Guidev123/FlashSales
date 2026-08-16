using Microsoft.EntityFrameworkCore;
using Modules.IntegrationTests.Abstractions;
using Modules.Launches.Application.Launches.Features.Activate;
using Modules.Launches.Application.Launches.Features.Create;
using Modules.Launches.Application.Launches.Features.Schedule;
using Modules.Launches.Application.Sellers.Features.Create;
using Modules.Orders.Application.Orders.Features.Confirm;
using Modules.Orders.Application.Orders.Features.Create;

namespace Modules.IntegrationTests.Orders.Outbox
{
    public sealed class OrdersOutboxTests(IntegrationWebApplicationFactory factory)
        : BaseOutboxTests(factory)
    {
        protected override DbContext ModuleDbContext => OrdersDbContext;
        protected override string Schema => "orders";

        protected override async Task SeedAsync()
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

            await Mediator.SendAsync(new ConfirmOrderCommand(createOrder.Value.OrderId));
        }
    }
}
