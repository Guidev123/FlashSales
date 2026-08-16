using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Modules.Payments.Application.Payments.DTOs;
using Modules.Payments.Domain.Payments.Enums;
using Modules.Payments.IntegrationTests.Abstractions;
using Modules.Payments.IntegrationTests.Abstractions.Helpers;
using System.Net;

namespace Modules.Payments.IntegrationTests.Features.Webhook
{
    public sealed class ConfirmPaymentWebhookTests(IntegrationWebApplicationFactory factory)
        : BaseIntegrationTest(factory), IAsyncLifetime
    {
        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync() => await _factory.ResetDatabaseAsync();

        [Fact]
        public async Task ConfirmPaymentWebhook_WithValidSignatureAndAuthorizedCharge_Returns200AndConfirmsAttempt()
        {
            // Arrange
            var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker);
            var payload = FakePaymentGatewayService.BuildValidSignedPayload(
                attempt.AttemptId, PaymentGatewayOutcome.Authorized, "ext_webhook", null, null, out var signature);

            using var client = _factory.CreateClient();
            using var content = new StringContent(payload);
            client.DefaultRequestHeaders.Add("Stripe-Signature", signature);

            // Act
            var response = await client.PostAsync("api/v1/payments/confirm", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var payment = await _dbContext.Payments.FirstAsync(p => p.Id == attempt.PaymentId);
            payment.Status.Should().Be(PaymentStatus.Completed);
        }

        [Fact]
        public async Task ConfirmPaymentWebhook_WithValidSignatureAndDeclinedCharge_Returns200AndDeclinesAttempt()
        {
            // Arrange
            var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker);
            var payload = FakePaymentGatewayService.BuildValidSignedPayload(
                attempt.AttemptId, PaymentGatewayOutcome.Declined, null, "card_declined", "Insufficient funds", out var signature);

            using var client = _factory.CreateClient();
            using var content = new StringContent(payload);
            client.DefaultRequestHeaders.Add("Stripe-Signature", signature);

            // Act
            var response = await client.PostAsync("api/v1/payments/confirm", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var payment = await _dbContext.Payments.Include(p => p.Attempts).FirstAsync(p => p.Id == attempt.PaymentId);
            payment.Attempts.Single().Status.Should().Be(PaymentAttemptStatus.Declined);
        }

        [Fact]
        public async Task ConfirmPaymentWebhook_WithInvalidSignature_Returns422AndDoesNotChangeAttempt()
        {
            // Arrange
            var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker);
            var payload = FakePaymentGatewayService.BuildValidSignedPayload(
                attempt.AttemptId, PaymentGatewayOutcome.Authorized, "ext", null, null, out _);

            using var client = _factory.CreateClient();
            using var content = new StringContent(payload);
            client.DefaultRequestHeaders.Add("Stripe-Signature", FakePaymentGatewayService.TamperedSignature);

            // Act
            var response = await client.PostAsync("api/v1/payments/confirm", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);

            var payment = await _dbContext.Payments.Include(p => p.Attempts).FirstAsync(p => p.Id == attempt.PaymentId);
            payment.Attempts.Single().Status.Should().Be(PaymentAttemptStatus.Initiated);
        }

        [Fact]
        public async Task ConfirmPaymentWebhook_WithUnrecognizedEventType_Returns200AndDoesNotDispatchCommand()
        {
            // Arrange
            var attempt = await PaymentHelper.CheckoutAsync(_factory, _faker);
            var payload = FakePaymentGatewayService.BuildUnrecognizedPayload(out var signature);

            using var client = _factory.CreateClient();
            using var content = new StringContent(payload);
            client.DefaultRequestHeaders.Add("Stripe-Signature", signature);

            // Act
            var response = await client.PostAsync("api/v1/payments/confirm", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var payment = await _dbContext.Payments.Include(p => p.Attempts).FirstAsync(p => p.Id == attempt.PaymentId);
            payment.Attempts.Single().Status.Should().Be(PaymentAttemptStatus.Initiated);
        }

        [Fact]
        public async Task ConfirmPaymentWebhook_WhenAttemptDoesNotExist_Returns404()
        {
            // Arrange
            var payload = FakePaymentGatewayService.BuildValidSignedPayload(
                Guid.NewGuid(), PaymentGatewayOutcome.Authorized, "ext", null, null, out var signature);

            using var client = _factory.CreateClient();
            using var content = new StringContent(payload);
            client.DefaultRequestHeaders.Add("Stripe-Signature", signature);

            // Act
            var response = await client.PostAsync("api/v1/payments/confirm", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
