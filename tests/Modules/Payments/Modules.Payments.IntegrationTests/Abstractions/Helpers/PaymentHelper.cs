using Bogus;
using Microsoft.Extensions.DependencyInjection;
using MidR.Interfaces;
using Modules.Payments.Application.Payments.DTOs;
using Modules.Payments.Application.Payments.Features.Checkout;

namespace Modules.Payments.IntegrationTests.Abstractions.Helpers
{
    internal static class PaymentHelper
    {
        internal static async Task<CheckoutPaymentResponse> CheckoutAsync(
            IntegrationWebApplicationFactory factory,
            Faker faker,
            Guid? orderId = null,
            decimal amount = 100m)
        {
            await using var scope = factory.Services.CreateAsyncScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

            var result = await mediator.SendAsync(new CheckoutPaymentCommand(
                orderId ?? Guid.NewGuid(),
                $"ORD-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}",
                amount,
                Guid.NewGuid(),
                faker.Internet.Email(),
                [new PaymentCheckoutLineItem(faker.Commerce.ProductName(), 1, amount)]));

            return result.Value;
        }
    }
}
