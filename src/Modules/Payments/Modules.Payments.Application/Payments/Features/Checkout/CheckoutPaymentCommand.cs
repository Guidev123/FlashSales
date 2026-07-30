using FlashSales.Application.Messaging;
using Modules.Payments.Application.Payments.DTOs;

namespace Modules.Payments.Application.Payments.Features.Checkout
{
    public sealed record CheckoutPaymentCommand(
        Guid OrderId,
        string OrderCode,
        decimal Amount,
        Guid CustomerId,
        string CustomerEmail,
        IReadOnlyCollection<PaymentCheckoutLineItem> Items
        ) : ICommand<CheckoutPaymentResponse>;
}