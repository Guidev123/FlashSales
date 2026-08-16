using FlashSales.Domain.Results;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MidR.Interfaces;
using Modules.Payments.Application.Payments.DTOs;
using Modules.Payments.Application.Payments.Features.Checkout;
using Modules.Payments.Application.Payments.Features.Confirm;
using Modules.Payments.Domain.Payments.Errors;
using Modules.Payments.IntegrationTests.Abstractions;
using Modules.Payments.IntegrationTests.Abstractions.Helpers;
using Npgsql;

namespace Modules.Payments.IntegrationTests.Features.Payments
{
    public sealed class CheckoutPaymentTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task CheckoutPayment_WhenNoPreviousPaymentExists_ShouldCreatePaymentAndReturnCheckoutUrl()
        {
            // Act
            var response = await PaymentHelper.CheckoutAsync(_factory, _faker);

            // Assert
            response.PaymentId.Should().NotBeEmpty();
            response.CheckoutUrl.Should().NotBeNullOrEmpty();

            var payment = await _dbContext.Payments.Include(p => p.Attempts).FirstAsync(p => p.Id == response.PaymentId);
            payment.Attempts.Should().HaveCount(1);
        }

        [Fact]
        public async Task CheckoutPayment_WhenPaymentAlreadyExistsForOrder_ShouldReuseExistingPaymentAndStartNewAttempt()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var first = await PaymentHelper.CheckoutAsync(_factory, _faker, orderId);
            await _mediator.SendAsync(new ConfirmPaymentCommand(first.AttemptId, PaymentGatewayOutcome.Pending, null, null, null));

            // Act
            var second = await PaymentHelper.CheckoutAsync(_factory, _faker, orderId);

            // Assert
            second.PaymentId.Should().Be(first.PaymentId);
            second.AttemptId.Should().NotBe(first.AttemptId);

            var payment = await _dbContext.Payments.Include(p => p.Attempts).FirstAsync(p => p.Id == first.PaymentId);
            payment.Attempts.Should().HaveCount(2);
        }

        [Fact]
        public async Task CheckoutPayment_WhenPaymentIsAlreadyCompleted_ShouldReturnFailure()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker, orderId);
            await _mediator.SendAsync(new ConfirmPaymentCommand(attempt.AttemptId, PaymentGatewayOutcome.Authorized, "ext_1", null, null));

            // Act
            var result = await _mediator.SendAsync(new CheckoutPaymentCommand(
                orderId, "ORD-AGAIN", 100m, Guid.NewGuid(), _faker.Internet.Email(),
                [new PaymentCheckoutLineItem("Item", 1, 100m)]));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(PaymentErrors.AlreadyPaid(orderId).Code);
        }

        [Fact]
        public async Task CheckoutPayment_WhenAttemptAlreadyInProgress_ShouldReturnFailure()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            await PaymentHelper.CheckoutAsync(_factory, _faker, orderId);

            // Act
            var result = await _mediator.SendAsync(new CheckoutPaymentCommand(
                orderId, "ORD-AGAIN", 100m, Guid.NewGuid(), _faker.Internet.Email(),
                [new PaymentCheckoutLineItem("Item", 1, 100m)]));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(PaymentErrors.AttemptInProgress(orderId).Code);
        }

        [Fact]
        public async Task CheckoutPayment_WhenMaxAttemptsReached_ShouldReturnFailure()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            for (var i = 0; i < 3; i++)
            {
                var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker, orderId);
                await _mediator.SendAsync(new ConfirmPaymentCommand(
                    attempt.AttemptId, PaymentGatewayOutcome.Declined, null, "card_declined", "Insufficient funds"));
            }

            // Act
            var result = await _mediator.SendAsync(new CheckoutPaymentCommand(
                orderId, "ORD-4TH", 100m, Guid.NewGuid(), _faker.Internet.Email(),
                [new PaymentCheckoutLineItem("Item", 1, 100m)]));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(PaymentErrors.MaxAttemptsReached(orderId).Code);
        }

        [Fact]
        public async Task CheckoutPayment_WhenPaymentWindowExpired_ShouldReturnFailure()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var first = await PaymentHelper.CheckoutAsync(_factory, _faker, orderId);
            await SendInNewScopeAsync(new ConfirmPaymentCommand(first.AttemptId, PaymentGatewayOutcome.Pending, null, null, null));
            await ForceExpiredAsync(first.PaymentId);

            // Act
            var result = await _mediator.SendAsync(new CheckoutPaymentCommand(
                orderId, "ORD-EXPIRED", 100m, Guid.NewGuid(), _faker.Internet.Email(),
                [new PaymentCheckoutLineItem("Item", 1, 100m)]));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(PaymentErrors.PaymentWindowExpired(orderId).Code);
        }

        [Fact]
        public async Task CheckoutPayment_WhenGatewayFailsToCreateSession_ShouldReturnFailureWithoutPersistingAttempt()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            _factory.PaymentGateway.EnqueueCheckoutFailure(PaymentErrors.CheckoutSessionCreationFailed);

            // Act
            var result = await _mediator.SendAsync(new CheckoutPaymentCommand(
                orderId, "ORD-GWFAIL", 100m, Guid.NewGuid(), _faker.Internet.Email(),
                [new PaymentCheckoutLineItem("Item", 1, 100m)]));

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error!.Code.Should().Be(PaymentErrors.CheckoutSessionCreationFailed.Code);

            var payment = await _dbContext.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
            payment.Should().BeNull();
        }

        [Fact]
        public async Task CheckoutPayment_WhenTwoConcurrentAttemptsForSameOrder_OnlyOneShouldSucceed()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var email = _faker.Internet.Email();

            await using var scope1 = _factory.Services.CreateAsyncScope();
            await using var scope2 = _factory.Services.CreateAsyncScope();

            var mediator1 = scope1.ServiceProvider.GetRequiredService<IMediator>();
            var mediator2 = scope2.ServiceProvider.GetRequiredService<IMediator>();

            // Act
            var task1 = mediator1.SendAsync(new CheckoutPaymentCommand(
                orderId, "ORD-C1", 100m, Guid.NewGuid(), email, [new PaymentCheckoutLineItem("Item", 1, 100m)]));
            var task2 = mediator2.SendAsync(new CheckoutPaymentCommand(
                orderId, "ORD-C2", 100m, Guid.NewGuid(), email, [new PaymentCheckoutLineItem("Item", 1, 100m)]));

            var results = await Task.WhenAll(task1, task2);

            // Assert
            results.Count(r => r.IsSuccess).Should().Be(1);
            results.Count(r => r.IsFailure).Should().Be(1);

            var paymentCount = await _dbContext.Payments.CountAsync(p => p.OrderId == orderId);
            paymentCount.Should().Be(1);
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
