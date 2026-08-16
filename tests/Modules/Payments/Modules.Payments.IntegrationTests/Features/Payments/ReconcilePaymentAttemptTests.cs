using FlashSales.Domain.Results;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Payments.Application.Payments.DTOs;
using Modules.Payments.Application.Payments.Features.Reconcile;
using Modules.Payments.Domain.Payments.Entities;
using Modules.Payments.Domain.Payments.Enums;
using Modules.Payments.Domain.Payments.Errors;
using Modules.Payments.Infrastructure.Database;
using Modules.Payments.IntegrationTests.Abstractions;
using Modules.Payments.IntegrationTests.Abstractions.Helpers;

namespace Modules.Payments.IntegrationTests.Features.Payments
{
    public sealed class ReconcilePaymentAttemptTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task ReconcilePaymentAttempt_WhenGatewayReportsAuthorized_ShouldCompletePayment()
        {
            // Arrange
            var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker);
            _factory.PaymentGateway.EnqueueReconciliationResult(PaymentGatewayOutcome.Authorized, "ext_recon");

            // Act
            var result = await _mediator.SendAsync(new ReconcilePaymentAttemptCommand(attempt.AttemptId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var payment = await _dbContext.Payments.FirstAsync(p => p.Id == attempt.PaymentId);
            payment.Status.Should().Be(PaymentStatus.Completed);
        }

        [Fact]
        public async Task ReconcilePaymentAttempt_WhenGatewayReportsDeclined_ShouldDeclineAttempt()
        {
            // Arrange
            var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker);
            _factory.PaymentGateway.EnqueueReconciliationResult(PaymentGatewayOutcome.Declined, null, "card_declined", "Insufficient funds");

            // Act
            var result = await _mediator.SendAsync(new ReconcilePaymentAttemptCommand(attempt.AttemptId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var payment = await _dbContext.Payments.Include(p => p.Attempts).FirstAsync(p => p.Id == attempt.PaymentId);
            payment.Attempts.Single().Status.Should().Be(PaymentAttemptStatus.Declined);
        }

        [Fact]
        public async Task ReconcilePaymentAttempt_WhenGatewayReportsPending_ShouldTimeOutAttempt()
        {
            // Arrange
            var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker);
            _factory.PaymentGateway.EnqueueReconciliationResult(PaymentGatewayOutcome.Pending);

            // Act
            var result = await _mediator.SendAsync(new ReconcilePaymentAttemptCommand(attempt.AttemptId));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var payment = await _dbContext.Payments.Include(p => p.Attempts).FirstAsync(p => p.Id == attempt.PaymentId);
            payment.Attempts.Single().Status.Should().Be(PaymentAttemptStatus.TimedOut);
        }

        [Fact]
        public async Task ReconcilePaymentAttempt_WhenAttemptHasNoGatewaySession_ShouldReturnNotFound()
        {
            // Arrange
            await using var scope = _factory.Services.CreateAsyncScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PaymentsDbContext>();

            var payment = Payment.Create(Guid.NewGuid(), 100m);
            var attemptResult = payment.StartAttempt();
            dbContext.Payments.Add(payment);
            await dbContext.SaveChangesAsync();

            // Act
            var result = await _mediator.SendAsync(new ReconcilePaymentAttemptCommand(attemptResult.Value.Id));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(PaymentErrors.AttemptNotFound(attemptResult.Value.Id).Code);
        }

        [Fact]
        public async Task ReconcilePaymentAttempt_WhenGatewayCheckFails_ShouldReturnFailure()
        {
            // Arrange
            var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker);
            _factory.PaymentGateway.EnqueueReconciliationFailure(Error.Problem("Payments.Test", "Gateway unreachable"));

            // Act
            var result = await _mediator.SendAsync(new ReconcilePaymentAttemptCommand(attempt.AttemptId));

            // Assert
            result.IsFailure.Should().BeTrue();
        }
    }
}
