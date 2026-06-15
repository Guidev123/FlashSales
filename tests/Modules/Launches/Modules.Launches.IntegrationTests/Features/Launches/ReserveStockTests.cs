using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MidR.Interfaces;
using Modules.Launches.Application.Launches.Features.ReserveStock;
using Modules.Launches.Domain.Launches.Enums;
using Modules.Launches.Domain.Launches.Errors;
using Modules.Launches.IntegrationTests.Abstractions;
using Modules.Launches.IntegrationTests.Abstractions.Helpers;

namespace Modules.Launches.IntegrationTests.Features.Launches
{
    public sealed class ReserveStockTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task ReserveStock_WhenLaunchIsActiveAndStockIsAvailable_ShouldDecrementAvailableQuantity()
        {
            // Arrange
            var (_, launchId) = await LaunchHelper.CreateAndActivateAsync(_factory, _faker, totalQuantity: 5);
            var orderId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new ReserveStockCommand(launchId, orderId, Quantity: 2));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var inDb = await _dbContext.Launches
                .Include(l => l.StockReservations)
                .FirstAsync(l => l.Id == launchId);

            inDb.Stock!.ReservedQuantity.Should().Be(2);
            inDb.Stock.AvailableQuantity.Should().Be(3);
            inDb.StockReservations.Should().ContainSingle(r => r.OrderId == orderId && r.Quantity == 2);
        }

        [Fact]
        public async Task ReserveStock_WhenLastUnitIsReserved_ShouldTransitionToSoldOut()
        {
            // Arrange
            var (_, launchId) = await LaunchHelper.CreateAndActivateAsync(_factory, _faker, totalQuantity: 1);

            // Act
            var result = await _mediator.SendAsync(new ReserveStockCommand(launchId, Guid.NewGuid(), Quantity: 1));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var inDb = await _dbContext.Launches.FindAsync(launchId);
            inDb!.Status.Should().Be(LaunchStatus.SoldOut);
            inDb.Stock!.AvailableQuantity.Should().Be(0);
        }

        [Fact]
        public async Task ReserveStock_WhenStockIsInsufficient_ShouldReturnFailure()
        {
            // Arrange
            var (_, launchId) = await LaunchHelper.CreateAndActivateAsync(_factory, _faker, totalQuantity: 2);

            // Act
            var result = await _mediator.SendAsync(new ReserveStockCommand(launchId, Guid.NewGuid(), Quantity: 5));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(LaunchErrors.InsufficientStock.Code);
        }

        [Fact]
        public async Task ReserveStock_WhenLaunchIsNotActive_ShouldReturnFailure()
        {
            // Arrange — launch is still Scheduled (not Active)
            var (_, launchId) = await LaunchHelper.CreateAndScheduleAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new ReserveStockCommand(launchId, Guid.NewGuid(), Quantity: 1));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(
                LaunchErrors.InvalidStatusTransition(LaunchStatus.Scheduled.ToString(), "ReserveStock").Code);
        }

        [Fact]
        public async Task ReserveStock_WhenSameOrderIdIsSubmittedTwice_ShouldBeIdempotent()
        {
            // Arrange
            var (_, launchId) = await LaunchHelper.CreateAndActivateAsync(_factory, _faker, totalQuantity: 10);
            var orderId = Guid.NewGuid();

            // Act — first reservation
            var firstResult = await _mediator.SendAsync(new ReserveStockCommand(launchId, orderId, Quantity: 2));

            // Act — duplicate submission with same orderId (retry scenario)
            var secondResult = await _mediator.SendAsync(new ReserveStockCommand(launchId, orderId, Quantity: 2));

            // Assert — both succeed but stock is only debited once
            firstResult.IsSuccess.Should().BeTrue();
            secondResult.IsSuccess.Should().BeTrue();

            var inDb = await _dbContext.Launches
                .Include(l => l.StockReservations)
                .FirstAsync(l => l.Id == launchId);

            inDb.Stock!.ReservedQuantity.Should().Be(2);
            inDb.StockReservations.Should().ContainSingle(r => r.OrderId == orderId);
        }

        [Fact]
        public async Task ReserveStock_WhenTwoConcurrentOrdersCompeteForLastUnit_OnlyOneShouldSucceed()
        {
            // Arrange — only 1 unit available; two different orders compete
            var (_, launchId) = await LaunchHelper.CreateAndActivateAsync(_factory, _faker, totalQuantity: 1);

            // Each scope has its own DbContext and UnitOfWork — true DB-level concurrency
            await using var scope1 = _factory.Services.CreateAsyncScope();
            await using var scope2 = _factory.Services.CreateAsyncScope();

            var mediator1 = scope1.ServiceProvider.GetRequiredService<IMediator>();
            var mediator2 = scope2.ServiceProvider.GetRequiredService<IMediator>();

            // Act
            var task1 = mediator1.SendAsync(new ReserveStockCommand(launchId, Guid.NewGuid(), Quantity: 1));
            var task2 = mediator2.SendAsync(new ReserveStockCommand(launchId, Guid.NewGuid(), Quantity: 1));

            var results = await Task.WhenAll(task1, task2);

            // Assert — exactly one succeeds; the other gets InsufficientStock after retries
            results.Count(r => r.IsSuccess).Should().Be(1);
            results.Count(r => r.IsFailure).Should().Be(1);
            results.Single(r => r.IsFailure).Error!.Code.Should().Be(LaunchErrors.InsufficientStock.Code);

            var inDb = await _dbContext.Launches
                .Include(l => l.StockReservations)
                .FirstAsync(l => l.Id == launchId);

            inDb.Stock!.ReservedQuantity.Should().Be(1);
            inDb.StockReservations.Should().HaveCount(1);
        }

        [Fact]
        public async Task ReserveStock_WhenLaunchDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new ReserveStockCommand(nonExistentId, Guid.NewGuid(), Quantity: 1));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(LaunchErrors.NotFound(nonExistentId).Code);
        }
    }
}
