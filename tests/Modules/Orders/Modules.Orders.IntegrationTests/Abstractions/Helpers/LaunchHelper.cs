using Bogus;
using Microsoft.Extensions.DependencyInjection;
using MidR.Interfaces;
using RealActivateLaunchCommand = Modules.Launches.Application.Launches.Features.Activate.ActivateLaunchCommand;
using RealCreateLaunchCommand = Modules.Launches.Application.Launches.Features.Create.CreateLaunchCommand;
using RealScheduleLaunchCommand = Modules.Launches.Application.Launches.Features.Schedule.ScheduleLaunchCommand;
using ReplicaCreateLaunchCommand = Modules.Orders.Application.Launches.Features.Create.CreateLaunchCommand;

namespace Modules.Orders.IntegrationTests.Abstractions.Helpers
{
    internal sealed record RealLaunch(
        Guid UserId,
        Guid SellerId,
        Guid ProductId,
        Guid LaunchId,
        string Title,
        decimal DiscountedPrice,
        decimal OriginalPrice,
        int TotalQuantity,
        DateTimeOffset StartAt,
        DateTimeOffset EndAt);

    internal static class LaunchHelper
    {
        internal const decimal DiscountedPrice = 50m;
        internal const decimal OriginalPrice = 100m;

        internal static async Task<RealLaunch> CreateActiveAsync(
            IntegrationWebApplicationFactory factory,
            Faker faker,
            int totalQuantity = 10)
        {
            var seller = await SellerHelper.CreateAsync(factory, faker);
            var productId = Guid.NewGuid();
            var title = faker.Commerce.ProductName();
            var startAt = DateTimeOffset.UtcNow.AddMinutes(5);
            var endAt = DateTimeOffset.UtcNow.AddHours(2);

            await using var createScope = factory.Services.CreateAsyncScope();
            var createMediator = createScope.ServiceProvider.GetRequiredService<IMediator>();

            var createResult = await createMediator.SendAsync(new RealCreateLaunchCommand(
                UserId: seller.UserId,
                ProductId: productId,
                Title: title,
                Description: faker.Commerce.ProductDescription()));

            var launchId = createResult.Value.Id;

            await using var scheduleScope = factory.Services.CreateAsyncScope();
            var scheduleMediator = scheduleScope.ServiceProvider.GetRequiredService<IMediator>();

            await scheduleMediator.SendAsync(new RealScheduleLaunchCommand(
                UserId: seller.UserId,
                LaunchId: launchId,
                DiscountedPrice: DiscountedPrice,
                OriginalPrice: OriginalPrice,
                TotalQuantity: totalQuantity,
                ReservedQuantity: 0,
                StartAt: startAt,
                EndAt: endAt));

            await using var activateScope = factory.Services.CreateAsyncScope();
            var activateMediator = activateScope.ServiceProvider.GetRequiredService<IMediator>();

            await activateMediator.SendAsync(new RealActivateLaunchCommand(launchId));

            await ReplicateAsync(
                factory,
                launchId,
                seller.SellerId,
                productId,
                title,
                DiscountedPrice,
                OriginalPrice,
                totalQuantity,
                startAt,
                endAt);

            return new RealLaunch(
                seller.UserId,
                seller.SellerId,
                productId,
                launchId,
                title,
                DiscountedPrice,
                OriginalPrice,
                totalQuantity,
                startAt,
                endAt);
        }

        internal static async Task ReplicateAsync(
            IntegrationWebApplicationFactory factory,
            Guid launchId,
            Guid sellerId,
            Guid productId,
            string title,
            decimal discountedPrice,
            decimal originalPrice,
            int totalQuantity,
            DateTimeOffset startAt,
            DateTimeOffset endAt)
        {
            await using var scope = factory.Services.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.SendAsync(new ReplicaCreateLaunchCommand(
                launchId,
                sellerId,
                productId,
                title,
                discountedPrice,
                originalPrice,
                totalQuantity,
                startAt,
                endAt));
        }
    }
}
