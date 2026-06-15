using Bogus;
using Microsoft.Extensions.DependencyInjection;
using MidR.Interfaces;
using Modules.Launches.Application.Launches.Features.Activate;
using Modules.Launches.Application.Launches.Features.Create;
using Modules.Launches.Application.Launches.Features.Schedule;

namespace Modules.Launches.IntegrationTests.Abstractions.Helpers
{
    internal static class LaunchHelper
    {
        internal static async Task<(Guid UserId, Guid LaunchId)> CreateAsync(
            IntegrationWebApplicationFactory factory,
            Faker faker)
        {
            var seller = await SellerHelper.CreateAsync(factory, faker);

            await using var scope = factory.Services.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var result = await mediator.SendAsync(new CreateLaunchCommand(
                UserId: seller.UserId,
                ProductId: Guid.NewGuid(),
                Title: faker.Commerce.ProductName(),
                Description: faker.Commerce.ProductDescription()));

            return (seller.UserId, result.Value.Id);
        }

        internal static async Task<(Guid UserId, Guid LaunchId)> CreateAndScheduleAsync(
            IntegrationWebApplicationFactory factory,
            Faker faker,
            int totalQuantity = 10)
        {
            var (userId, launchId) = await CreateAsync(factory, faker);

            await using var scope = factory.Services.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.SendAsync(new ScheduleLaunchCommand(
                UserId: userId,
                LaunchId: launchId,
                DiscountedPrice: 50m,
                OriginalPrice: 100m,
                TotalQuantity: totalQuantity,
                ReservedQuantity: 0,
                StartAt: DateTimeOffset.UtcNow.AddMinutes(5),
                EndAt: DateTimeOffset.UtcNow.AddHours(2)));

            return (userId, launchId);
        }

        internal static async Task<(Guid UserId, Guid LaunchId)> CreateAndActivateAsync(
            IntegrationWebApplicationFactory factory,
            Faker faker,
            int totalQuantity = 10)
        {
            var (userId, launchId) = await CreateAndScheduleAsync(factory, faker, totalQuantity);

            await using var scope = factory.Services.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            await mediator.SendAsync(new ActivateLaunchCommand(launchId));

            return (userId, launchId);
        }
    }
}
