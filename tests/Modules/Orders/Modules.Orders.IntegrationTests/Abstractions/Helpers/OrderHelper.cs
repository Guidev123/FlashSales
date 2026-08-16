using Bogus;
using FlashSales.Domain.Results;
using Microsoft.Extensions.DependencyInjection;
using MidR.Interfaces;
using Modules.Launches.Application.Launches.Features.ReserveStock;
using Modules.Orders.Application.Orders.Features.Create;
using Modules.Orders.Domain.Orders.Entities;
using Modules.Orders.Domain.Orders.Models;
using Modules.Orders.Infrastructure.Database;

namespace Modules.Orders.IntegrationTests.Abstractions.Helpers
{
    internal static class OrderHelper
    {
        internal static async Task<(RealLaunch Launch, Guid CustomerId, Result<CreateOrderResponse> Result)> CreateAsync(
            IntegrationWebApplicationFactory factory,
            Faker faker,
            Guid? customerId = null,
            int launchTotalQuantity = 10,
            int quantity = 1)
        {
            var launch = await LaunchHelper.CreateActiveAsync(factory, faker, launchTotalQuantity);
            var custId = customerId ?? Guid.NewGuid();

            await using var scope = factory.Services.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var result = await mediator.SendAsync(new CreateOrderCommand(
                custId,
                faker.Internet.Email(),
                launch.LaunchId,
                quantity));

            return (launch, custId, result);
        }

        internal static async Task<(RealLaunch Launch, Guid CustomerId, Guid OrderId, string OrderCode)> CreateAwaitingPaymentAsync(
            IntegrationWebApplicationFactory factory,
            Faker faker,
            Guid? customerId = null,
            int launchTotalQuantity = 10,
            int quantity = 1)
        {
            var (launch, custId, result) = await CreateAsync(factory, faker, customerId, launchTotalQuantity, quantity);
            return (launch, custId, result.Value.OrderId, result.Value.OrderCode);
        }

        internal static async Task<(RealLaunch Launch, Guid CustomerId, Guid OrderId)> CreateStuckAtInitiatingCheckoutAsync(
            IntegrationWebApplicationFactory factory,
            Faker faker,
            int launchTotalQuantity = 10,
            int quantity = 1,
            bool reserveRealStock = true)
        {
            var launch = await LaunchHelper.CreateActiveAsync(factory, faker, launchTotalQuantity);
            var customerId = Guid.NewGuid();

            await using (var scope = factory.Services.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

                var order = Order.Create(customerId, launch.LaunchId, launch.SellerId, launch.ProductId, quantity, launch.DiscountedPrice);
                var saga = OrderCreationSaga.Create(order.Id);
                saga.MarkStockReserved();

                dbContext.Orders.Add(order);
                dbContext.OrderCreationSagas.Add(saga);
                await dbContext.SaveChangesAsync();

                if (reserveRealStock)
                {
                    await using var reserveScope = factory.Services.CreateAsyncScope();
                    var mediator = reserveScope.ServiceProvider.GetRequiredService<IMediator>();
                    await mediator.SendAsync(new ReserveStockCommand(launch.LaunchId, order.Id, quantity));
                }

                return (launch, customerId, order.Id);
            }
        }
    }
}
