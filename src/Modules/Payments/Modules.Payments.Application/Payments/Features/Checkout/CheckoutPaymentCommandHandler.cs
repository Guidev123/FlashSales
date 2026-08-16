using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Payments.Application.Payments.DTOs;
using Modules.Payments.Application.Payments.Services;
using Modules.Payments.Domain.Payments.Entities;
using Modules.Payments.Domain.Payments.Repositories;

namespace Modules.Payments.Application.Payments.Features.Checkout
{
    internal sealed class CheckoutPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentGatewayService paymentGatewayService
        ) : ICommandHandler<CheckoutPaymentCommand, CheckoutPaymentResponse>
    {
        public async Task<Result<CheckoutPaymentResponse>> ExecuteAsync(CheckoutPaymentCommand request, CancellationToken cancellationToken = default)
        {
            var payment = await paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);

            if (payment is null)
            {
                payment = Payment.Create(request.OrderId, request.Amount);
                paymentRepository.Add(payment);
            }

            var attemptResult = payment.StartAttempt();
            if (attemptResult.IsFailure)
            {
                return Result.Failure<CheckoutPaymentResponse>(attemptResult.Error!);
            }

            var attempt = attemptResult.Value;

            var sessionResult = await paymentGatewayService.CreateCheckoutSessionAsync(
                new PaymentCheckoutRequest(
                    attempt.Id,
                    request.CustomerId,
                    request.CustomerEmail,
                    request.OrderCode,
                    payment.Amount,
                    request.Items),
                cancellationToken);

            if (sessionResult.IsFailure)
            {
                return Result.Failure<CheckoutPaymentResponse>(sessionResult.Error!);
            }

            attempt.AttachGatewaySession(sessionResult.Value.SessionId);

            paymentRepository.Update(payment);

            return new CheckoutPaymentResponse(payment.Id, attempt.Id, sessionResult.Value.CheckoutUrl);
        }
    }
}