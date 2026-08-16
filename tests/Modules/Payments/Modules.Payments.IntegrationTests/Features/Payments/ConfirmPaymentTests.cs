using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Payments.Application.Payments.DTOs;
using Modules.Payments.Application.Payments.Features.Checkout;
using Modules.Payments.Application.Payments.Features.Confirm;
using Modules.Payments.Domain.Payments.Enums;
using Modules.Payments.Domain.Payments.Errors;
using Modules.Payments.IntegrationTests.Abstractions;
using Modules.Payments.IntegrationTests.Abstractions.Helpers;
using Npgsql;

namespace Modules.Payments.IntegrationTests.Features.Payments
{
    public sealed class ConfirmPaymentTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task ConfirmPayment_WhenOutcomeIsAuthorizedWithinWindow_ShouldCompletePayment()
        {
            // Arrange
            var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new ConfirmPaymentCommand(
                attempt.AttemptId, PaymentGatewayOutcome.Authorized, "ext_123", null, null));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var payment = await _dbContext.Payments.Include(p => p.Attempts).FirstAsync(p => p.Id == attempt.PaymentId);
            payment.Status.Should().Be(PaymentStatus.Completed);
            payment.Attempts.Single().Status.Should().Be(PaymentAttemptStatus.Authorized);
        }

        [Fact]
        public async Task ConfirmPayment_WhenOutcomeIsAuthorizedAfterWindowExpired_ShouldRefundAndMarkRefunded()
        {
            // Arrange
            var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker);
            await ForceExpiredAsync(attempt.PaymentId);
            _factory.PaymentGateway.EnqueueRefundSuccess();

            // Act
            var result = await _mediator.SendAsync(new ConfirmPaymentCommand(
                attempt.AttemptId, PaymentGatewayOutcome.Authorized, "ext_123", null, null));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var payment = await _dbContext.Payments.FirstAsync(p => p.Id == attempt.PaymentId);
            payment.Status.Should().Be(PaymentStatus.Refunded);
        }

        [Fact]
        public async Task ConfirmPayment_WhenOutcomeIsAuthorizedAfterWindowExpiredAndRefundFails_ShouldReturnFailure()
        {
            // Arrange
            var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker);
            await ForceExpiredAsync(attempt.PaymentId);
            _factory.PaymentGateway.EnqueueRefundFailure();

            // Act
            var result = await _mediator.SendAsync(new ConfirmPaymentCommand(
                attempt.AttemptId, PaymentGatewayOutcome.Authorized, "ext_123", null, null));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(PaymentErrors.RefundFailed.Code);
        }

        [Fact]
        public async Task ConfirmPayment_WhenOutcomeIsDeclined_ShouldDeclineAttempt()
        {
            // Arrange
            var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new ConfirmPaymentCommand(
                attempt.AttemptId, PaymentGatewayOutcome.Declined, null, "card_declined", "Insufficient funds"));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var payment = await _dbContext.Payments.Include(p => p.Attempts).FirstAsync(p => p.Id == attempt.PaymentId);
            payment.Attempts.Single().Status.Should().Be(PaymentAttemptStatus.Declined);
            payment.Status.Should().Be(PaymentStatus.Pending);
        }

        [Fact]
        public async Task ConfirmPayment_WhenDeclinedReachesMaxAttempts_ShouldMarkPaymentFailed()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var attempt1 = await PaymentHelper.CheckoutAsync(_factory, _faker, orderId);
            await _mediator.SendAsync(new ConfirmPaymentCommand(
                attempt1.AttemptId, PaymentGatewayOutcome.Declined, null, "card_declined", "Insufficient funds"));
            var attempt2 = await PaymentHelper.CheckoutAsync(_factory, _faker, orderId);
            await _mediator.SendAsync(new ConfirmPaymentCommand(
                attempt2.AttemptId, PaymentGatewayOutcome.Declined, null, "card_declined", "Insufficient funds"));
            var attempt3 = await PaymentHelper.CheckoutAsync(_factory, _faker, orderId);

            // Act
            var result = await _mediator.SendAsync(new ConfirmPaymentCommand(
                attempt3.AttemptId, PaymentGatewayOutcome.Declined, null, "card_declined", "Insufficient funds"));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var payment = await _dbContext.Payments.FirstAsync(p => p.Id == attempt3.PaymentId);
            payment.Status.Should().Be(PaymentStatus.Failed);
        }

        [Fact]
        public async Task ConfirmPayment_WhenOutcomeIsPending_ShouldTimeOutAttempt()
        {
            // Arrange
            var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker);

            // Act
            var result = await _mediator.SendAsync(new ConfirmPaymentCommand(
                attempt.AttemptId, PaymentGatewayOutcome.Pending, null, null, null));

            // Assert
            result.IsSuccess.Should().BeTrue();

            var payment = await _dbContext.Payments.Include(p => p.Attempts).FirstAsync(p => p.Id == attempt.PaymentId);
            payment.Attempts.Single().Status.Should().Be(PaymentAttemptStatus.TimedOut);
        }

        [Fact]
        public async Task ConfirmPayment_WhenAttemptDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var attemptId = Guid.NewGuid();

            // Act
            var result = await _mediator.SendAsync(new ConfirmPaymentCommand(
                attemptId, PaymentGatewayOutcome.Authorized, "ext", null, null));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(PaymentErrors.AttemptNotFound(attemptId).Code);
        }

        [Fact]
        public async Task ConfirmPayment_WhenCalledTwiceForSameAuthorizedAttempt_ShouldBeIdempotent()
        {
            // Arrange
            var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker);
            var firstResult = await _mediator.SendAsync(new ConfirmPaymentCommand(
                attempt.AttemptId, PaymentGatewayOutcome.Authorized, "ext_123", null, null));

            // Act
            var secondResult = await _mediator.SendAsync(new ConfirmPaymentCommand(
                attempt.AttemptId, PaymentGatewayOutcome.Authorized, "ext_123", null, null));

            // Assert
            firstResult.IsSuccess.Should().BeTrue();
            secondResult.IsSuccess.Should().BeTrue();
        }

        private async Task ForceExpiredAsync(Guid paymentId)
        {
            await using var connection = new NpgsqlConnection(_factory.GetConnectionString());
            await connection.OpenAsync();

            await using var cmd = new NpgsqlCommand(
                """UPDATE payments."Payments" SET "ExpiresAt" = now() - interval '1 hour' WHERE "Id" = @PaymentId""",
                connection);
            cmd.Parameters.AddWithValue("PaymentId", paymentId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
