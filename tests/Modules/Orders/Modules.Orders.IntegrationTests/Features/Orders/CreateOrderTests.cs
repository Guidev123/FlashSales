using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using FlashSales.Domain.Results;
using MidR.Interfaces;
using Modules.Launches.Application.Launches.Features.ReserveStock;
using Modules.Launches.Domain.Launches.Enums;
using Modules.Launches.Domain.Launches.Errors;
using Modules.Orders.Application.Launches.Features.End;
using Modules.Orders.Application.Orders.Features.Confirm;
using Modules.Orders.Application.Orders.Features.Create;
using Modules.Orders.Domain.Orders.Enums;
using Modules.Orders.Domain.Orders.Errors;
using Modules.Orders.Domain.Orders.Models;
using Modules.Orders.IntegrationTests.Abstractions;
using Modules.Orders.IntegrationTests.Abstractions.Helpers;

namespace Modules.Orders.IntegrationTests.Features.Orders
{
    public sealed class CreateOrderTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task CreateOrder_WhenLaunchIsActiveAndValid_ShouldReturnCheckoutUrlAndTransitionOrderToPaymentProcessing()
        {
            // Arrange
            var launch = await LaunchHelper.CreateActiveAsync(_factory, _faker, totalQuantity: 10);
            var customerId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new CreateOrderCommand(customerId, _faker.Internet.Email(), launch.LaunchId, Quantity: 2));

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.OrderId.Should().NotBeEmpty();
            result.Value.CheckoutUrl.Should().NotBeNullOrEmpty();

            var order = await _dbContext.Orders.FirstAsync(o => o.Id == result.Value.OrderId);
            order.Status.Should().Be(OrderStatus.PaymentProcessing);
            order.Quantity.Should().Be(2);

            var realLaunch = await _launchesDbContext.Launches.FirstAsync(l => l.Id == launch.LaunchId);
            realLaunch.Stock!.ReservedQuantity.Should().Be(2);
        }

        [Fact]
        public async Task CreateOrder_WhenLaunchDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var launchId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new CreateOrderCommand(Guid.NewGuid(), _faker.Internet.Email(), launchId, 1));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(OrderErrors.LaunchNotFound(launchId).Code);
        }

        [Fact]
        public async Task CreateOrder_WhenLaunchIsNotActive_ShouldReturnFailure()
        {
            // Arrange
            var launchId = Guid.NewGuid();
            await LaunchHelper.ReplicateAsync(
                _factory, launchId, Guid.NewGuid(), Guid.NewGuid(), _faker.Commerce.ProductName(),
                50m, 100m, 10, DateTimeOffset.UtcNow.AddMinutes(5), DateTimeOffset.UtcNow.AddHours(2));
            await _mediator.SendAsync(new EndLaunchCommand(launchId));

            // Act
            var result = await _mediator.SendAsync(new CreateOrderCommand(Guid.NewGuid(), _faker.Internet.Email(), launchId, 1));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(OrderErrors.LaunchNotActive(launchId).Code);
        }

        [Fact]
        public async Task CreateOrder_WhenCustomerAlreadyHasActiveOrderForLaunch_ShouldReturnConflict()
        {
            // Arrange
            var (launch, customerId, firstResult) = await OrderHelper.CreateAsync(_factory, _faker, launchTotalQuantity: 10, quantity: 1);
            firstResult.IsSuccess.Should().BeTrue();

            // Act
            var result = await _mediator.SendAsync(new CreateOrderCommand(customerId, _faker.Internet.Email(), launch.LaunchId, 1));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(OrderErrors.ActiveOrderAlreadyExists(launch.LaunchId).Code);
        }

        [Fact]
        public async Task CreateOrder_WhenConfirmedQuantityPlusNewQuantityExceedsGlobalUnitLimit_ShouldReturnFailure()
        {
            // Arrange
            var (launch, customerId, firstResult) = await OrderHelper.CreateAsync(_factory, _faker, launchTotalQuantity: 10, quantity: 5);
            firstResult.IsSuccess.Should().BeTrue();
            await _mediator.SendAsync(new ConfirmOrderCommand(firstResult.Value.OrderId));

            // Act
            var result = await _mediator.SendAsync(new CreateOrderCommand(customerId, _faker.Internet.Email(), launch.LaunchId, 1));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(OrderErrors.UnitLimitExceeded(5).Code);
        }

        [Fact]
        public async Task CreateOrder_WhenConfirmedQuantityExactlyReachesGlobalUnitLimit_ShouldSucceed()
        {
            // Arrange
            var (launch, customerId, firstResult) = await OrderHelper.CreateAsync(_factory, _faker, launchTotalQuantity: 10, quantity: 3);
            firstResult.IsSuccess.Should().BeTrue();
            await _mediator.SendAsync(new ConfirmOrderCommand(firstResult.Value.OrderId));

            // Act
            var result = await SendInNewScopeAsync(new CreateOrderCommand(customerId, _faker.Internet.Email(), launch.LaunchId, 2));

            // Assert
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task CreateOrder_WhenStockReservationFails_ShouldCancelOrderAndReturnFailure()
        {
            // Arrange
            var launch = await LaunchHelper.CreateActiveAsync(_factory, _faker, totalQuantity: 1);
            await SendInNewScopeAsync(new ReserveStockCommand(launch.LaunchId, Guid.NewGuid(), Quantity: 1));

            var customerId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new CreateOrderCommand(customerId, _faker.Internet.Email(), launch.LaunchId, 1));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(LaunchErrors.InsufficientStock.Code);

            var order = await _dbContext.Orders.FirstAsync(o => o.CustomerId == customerId && o.LaunchId == launch.LaunchId);
            order.Status.Should().Be(OrderStatus.Cancelled);

            var saga = await _dbContext.OrderCreationSagas.FirstAsync(s => s.Id == order.Id);
            saga.Step.Should().Be(OrderCreationSagaStep.Failed);
        }

        [Fact]
        public async Task CreateOrder_WhenCheckoutInitiationFails_ShouldReleaseStockAndCancelOrder()
        {
            // Arrange
            var launch = await LaunchHelper.CreateActiveAsync(_factory, _faker, totalQuantity: 5);
            _factory.PaymentGateway.EnqueueCheckoutFailure(Error.Problem("Payments.Test", "Simulated gateway outage"));

            var customerId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new CreateOrderCommand(customerId, _faker.Internet.Email(), launch.LaunchId, 2));

            // Assert
            result.IsFailure.Should().BeTrue();

            var order = await _dbContext.Orders.FirstAsync(o => o.CustomerId == customerId && o.LaunchId == launch.LaunchId);
            order.Status.Should().Be(OrderStatus.Cancelled);

            var saga = await _dbContext.OrderCreationSagas.FirstAsync(s => s.Id == order.Id);
            saga.Step.Should().Be(OrderCreationSagaStep.Compensated);

            var realLaunch = await _launchesDbContext.Launches.FirstAsync(l => l.Id == launch.LaunchId);
            realLaunch.Stock!.ReservedQuantity.Should().Be(0);
        }

        [Fact]
        public async Task CreateOrder_WhenTwoConcurrentRequestsForSameCustomerAndLaunch_OnlyOneShouldSucceed()
        {
            // Arrange
            var launch = await LaunchHelper.CreateActiveAsync(_factory, _faker, totalQuantity: 10);
            var customerId = Guid.NewGuid();
            var email = _faker.Internet.Email();

            await using var scope1 = _factory.Services.CreateAsyncScope();
            await using var scope2 = _factory.Services.CreateAsyncScope();

            var mediator1 = scope1.ServiceProvider.GetRequiredService<IMediator>();
            var mediator2 = scope2.ServiceProvider.GetRequiredService<IMediator>();

            // Act
            var task1 = mediator1.SendAsync(new CreateOrderCommand(customerId, email, launch.LaunchId, 1));
            var task2 = mediator2.SendAsync(new CreateOrderCommand(customerId, email, launch.LaunchId, 1));

            var results = await Task.WhenAll(task1, task2);

            // Assert
            results.Count(r => r.IsSuccess).Should().Be(1);
            results.Count(r => r.IsFailure).Should().Be(1);
            results.Single(r => r.IsFailure).Error!.Code.Should().Be(OrderErrors.ActiveOrderAlreadyExists(launch.LaunchId).Code);

            var orderCount = await _dbContext.Orders.CountAsync(o => o.CustomerId == customerId && o.LaunchId == launch.LaunchId);
            orderCount.Should().Be(1);
        }

        [Fact]
        public async Task CreateOrder_WhenTwoConcurrentOrdersFromDifferentCustomersCompeteForLastUnit_OnlyOneShouldSucceedAndStockIsNotOversold()
        {
            // Arrange
            var launch = await LaunchHelper.CreateActiveAsync(_factory, _faker, totalQuantity: 1);
            var customer1 = Guid.NewGuid();
            var customer2 = Guid.NewGuid();

            await using var scope1 = _factory.Services.CreateAsyncScope();
            await using var scope2 = _factory.Services.CreateAsyncScope();

            var mediator1 = scope1.ServiceProvider.GetRequiredService<IMediator>();
            var mediator2 = scope2.ServiceProvider.GetRequiredService<IMediator>();

            // Act
            var task1 = mediator1.SendAsync(new CreateOrderCommand(customer1, _faker.Internet.Email(), launch.LaunchId, 1));
            var task2 = mediator2.SendAsync(new CreateOrderCommand(customer2, _faker.Internet.Email(), launch.LaunchId, 1));

            var results = await Task.WhenAll(task1, task2);

            // Assert
            results.Count(r => r.IsSuccess).Should().Be(1);
            results.Count(r => r.IsFailure).Should().Be(1);

            var orders = await _dbContext.Orders.Where(o => o.LaunchId == launch.LaunchId).ToListAsync();
            orders.Should().HaveCount(2);
            orders.Count(o => o.Status == OrderStatus.PaymentProcessing).Should().Be(1);
            orders.Count(o => o.Status == OrderStatus.Cancelled).Should().Be(1);

            var realLaunch = await _launchesDbContext.Launches.FirstAsync(l => l.Id == launch.LaunchId);
            realLaunch.Stock!.ReservedQuantity.Should().Be(1);
            realLaunch.Status.Should().Be(LaunchStatus.SoldOut);
        }
    }
}
