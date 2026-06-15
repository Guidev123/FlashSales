using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Launches.Application.Launches.Features.ReleaseStock;
using Modules.Launches.Application.Launches.Features.ReserveStock;
using Modules.Launches.Domain.Launches.Enums;
using Modules.Launches.Domain.Launches.Errors;
using Modules.Launches.IntegrationTests.Abstractions;
using Modules.Launches.IntegrationTests.Abstractions.Helpers;

namespace Modules.Launches.IntegrationTests.Features.Launches
{
    public sealed class ReleaseStockTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task ReleaseStock_WhenReservationExists_ShouldRestoreAvailableQuantity()
        {
            // Arrange
            var (_, launchId) = await LaunchHelper.CreateAndActivateAsync(_factory, _faker, totalQuantity: 5);
            var orderId = Guid.NewGuid();

            // Reserve in a separate scope — each write to the same aggregate needs its own scope
            // to avoid stale xmin in the EF change tracker (xmin is updated by PostgreSQL on every
            // write but EF does not refresh it without an explicit RETURNING clause).
            await SendInNewScopeAsync(new ReserveStockCommand(launchId, orderId, Quantity: 3));

            // Act — Release now loads a fresh entity (not in the current tracker) → correct xmin
            var result = await _mediator.SendAsync(new ReleaseStockCommand(launchId, orderId, Quantity: 3));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var inDb = await _dbContext.Launches
                .Include(l => l.StockReservations)
                .FirstAsync(l => l.Id == launchId);

            inDb.Stock!.ReservedQuantity.Should().Be(0);
            inDb.Stock.AvailableQuantity.Should().Be(5);
            inDb.StockReservations.Should().BeEmpty();
        }

        [Fact]
        public async Task ReleaseStock_WhenLaunchWasSoldOut_ShouldTransitionBackToActive()
        {
            // Arrange — reserve the last unit → SoldOut
            var (_, launchId) = await LaunchHelper.CreateAndActivateAsync(_factory, _faker, totalQuantity: 1);
            var orderId = Guid.NewGuid();

            await SendInNewScopeAsync(new ReserveStockCommand(launchId, orderId, Quantity: 1));

            // Verify SoldOut via a DB read (entity not in current tracker since reserve ran in its own scope)
            var soldOut = await _dbContext.Launches.FindAsync(launchId);
            soldOut!.Status.Should().Be(LaunchStatus.SoldOut);

            // Act — release the reservation (e.g., order cancelled)
            var result = await _mediator.SendAsync(new ReleaseStockCommand(launchId, orderId, Quantity: 1));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var inDb = await _dbContext.Launches.FindAsync(launchId);
            inDb!.Status.Should().Be(LaunchStatus.Active);
            inDb.Stock!.AvailableQuantity.Should().Be(1);
        }

        [Fact]
        public async Task ReleaseStock_WhenReservationDoesNotExist_ShouldBeIdempotent()
        {
            // Arrange — release without a prior reservation (already released or never existed)
            var (_, launchId) = await LaunchHelper.CreateAndActivateAsync(_factory, _faker, totalQuantity: 5);
            var orderId = Guid.NewGuid();

            // Act — idempotency: releasing a non-existent reservation is a no-op
            var result = await _mediator.SendAsync(new ReleaseStockCommand(launchId, orderId, Quantity: 1));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var inDb = await _dbContext.Launches.FindAsync(launchId);
            inDb!.Stock!.ReservedQuantity.Should().Be(0);
        }

        [Fact]
        public async Task ReleaseStock_WhenCalledTwiceForSameOrder_ShouldBeIdempotent()
        {
            // Arrange
            var (_, launchId) = await LaunchHelper.CreateAndActivateAsync(_factory, _faker, totalQuantity: 5);
            var orderId = Guid.NewGuid();

            await SendInNewScopeAsync(new ReserveStockCommand(launchId, orderId, Quantity: 2));

            // Act — first release: entity not in current tracker → loads fresh from DB
            var firstResult = await _mediator.SendAsync(new ReleaseStockCommand(launchId, orderId, Quantity: 2));

            // Act — second release: domain-level idempotency kicks in (StockReservation already
            // removed from in-memory collection) → returns Success without touching the DB
            var secondResult = await _mediator.SendAsync(new ReleaseStockCommand(launchId, orderId, Quantity: 2));

            // Assert — both succeed; stock is only restored once
            firstResult.IsSuccess.Should().BeTrue();
            secondResult.IsSuccess.Should().BeTrue();

            var inDb = await _dbContext.Launches
                .Include(l => l.StockReservations)
                .FirstAsync(l => l.Id == launchId);

            inDb.Stock!.ReservedQuantity.Should().Be(0);
            inDb.StockReservations.Should().BeEmpty();
        }

        [Fact]
        public async Task ReleaseStock_WhenLaunchDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new ReleaseStockCommand(nonExistentId, Guid.NewGuid(), Quantity: 1));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(LaunchErrors.NotFound(nonExistentId).Code);
        }
    }
}
