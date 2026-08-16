using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Payments.Application.Payments.DTOs;
using Modules.Payments.Application.Payments.Features.Confirm;
using Modules.Payments.Application.Payments.Features.Reconcile;
using Modules.Payments.Domain.Payments.Enums;
using Modules.Payments.Domain.Payments.Repositories;
using Modules.Payments.IntegrationTests.Abstractions;
using Modules.Payments.IntegrationTests.Abstractions.Helpers;
using Npgsql;

namespace Modules.Payments.IntegrationTests.Features.Payments
{
    public sealed class PaymentReconciliationSweepTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task GetStaleInitiatedAttemptIdsAsync_WhenAttemptIsOlderThanStaleness_ShouldReturnAttemptId()
        {
            // Arrange
            var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker);
            await ForceAttemptCreatedOnAsync(attempt.AttemptId, DateTimeOffset.UtcNow.AddHours(-1));

            await using var scope = _factory.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

            // Act
            var staleIds = await repository.GetStaleInitiatedAttemptIdsAsync(TimeSpan.FromSeconds(60));

            // Assert
            staleIds.Should().Contain(attempt.AttemptId);
        }

        [Fact]
        public async Task GetStaleInitiatedAttemptIdsAsync_WhenAttemptIsAuthorizedOrDeclined_ShouldNotReturnIt()
        {
            // Arrange
            var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker);
            await _mediator.SendAsync(new ConfirmPaymentCommand(attempt.AttemptId, PaymentGatewayOutcome.Authorized, "ext", null, null));
            await ForceAttemptCreatedOnAsync(attempt.AttemptId, DateTimeOffset.UtcNow.AddHours(-1));

            await using var scope = _factory.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();

            // Act
            var staleIds = await repository.GetStaleInitiatedAttemptIdsAsync(TimeSpan.FromSeconds(60));

            // Assert
            staleIds.Should().NotContain(attempt.AttemptId);
        }

        [Fact]
        public async Task ReconcilePaymentAttemptCommand_SimulatingSweepJobBehavior_ShouldReconcileStaleAttempt()
        {
            // Arrange
            var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker);
            await ForceAttemptCreatedOnAsync(attempt.AttemptId, DateTimeOffset.UtcNow.AddHours(-1));
            _factory.PaymentGateway.EnqueueReconciliationResult(PaymentGatewayOutcome.Authorized, "ext_sweep");

            await using var scope = _factory.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IPaymentRepository>();
            var staleIds = await repository.GetStaleInitiatedAttemptIdsAsync(TimeSpan.FromSeconds(60));
            staleIds.Should().Contain(attempt.AttemptId);

            // Act
            var result = await _mediator.SendAsync(new ReconcilePaymentAttemptCommand(attempt.AttemptId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var payment = await _dbContext.Payments.FirstAsync(p => p.Id == attempt.PaymentId);
            payment.Status.Should().Be(PaymentStatus.Completed);
        }

        private async Task ForceAttemptCreatedOnAsync(Guid attemptId, DateTimeOffset createdOn)
        {
            await using var connection = new NpgsqlConnection(_factory.GetConnectionString());
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                """UPDATE payments."PaymentAttempts" SET "CreatedOn" = @CreatedOn WHERE "Id" = @AttemptId""",
                connection);
            cmd.Parameters.AddWithValue("CreatedOn", createdOn);
            cmd.Parameters.AddWithValue("AttemptId", attemptId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
