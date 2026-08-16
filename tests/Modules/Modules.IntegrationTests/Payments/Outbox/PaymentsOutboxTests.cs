using Microsoft.EntityFrameworkCore;
using Modules.IntegrationTests.Abstractions;
using Modules.Payments.Application.Payments.DTOs;
using Modules.Payments.Application.Payments.Features.Checkout;
using Modules.Payments.Application.Payments.Features.Confirm;
using Npgsql;

namespace Modules.IntegrationTests.Payments.Outbox
{
    public sealed class PaymentsOutboxTests(IntegrationWebApplicationFactory factory)
        : BaseOutboxConsumerTrackingTests(factory)
    {
        protected override DbContext ModuleDbContext => PaymentsDbContext;
        protected override string Schema => "payments";

        protected override async Task SeedAsync()
        {
            var checkout = await Mediator.SendAsync(new CheckoutPaymentCommand(
                Guid.NewGuid(),
                $"ORD-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                100m,
                Guid.NewGuid(),
                Faker.Internet.Email(),
                [new PaymentCheckoutLineItem(Faker.Commerce.ProductName(), 1, 100m)]));

            await using var connection = new NpgsqlConnection(Factory.GetConnectionString());
            await connection.OpenAsync();
            await using var purge = new NpgsqlCommand(
                """DELETE FROM payments."OutboxMessages" WHERE "Type" LIKE '%PaymentCreatedDomainEvent%'""",
                connection);
            await purge.ExecuteNonQueryAsync();

            await Mediator.SendAsync(new ConfirmPaymentCommand(
                checkout.Value.AttemptId, PaymentGatewayOutcome.Authorized, $"ext_{Guid.NewGuid():N}", null, null));
        }
    }
}
