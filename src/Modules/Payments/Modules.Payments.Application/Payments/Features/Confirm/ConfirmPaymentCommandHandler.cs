using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Payments.Application.Payments.Services;
using Modules.Payments.Domain.Payments.Errors;
using Modules.Payments.Domain.Payments.Repositories;

namespace Modules.Payments.Application.Payments.Features.Confirm
{
    internal sealed class ConfirmPaymentCommandHandler(
        IPaymentRepository paymentRepository,
        PaymentOutcomeProcessor processor
        ) : ICommandHandler<ConfirmPaymentCommand>
    {
        public async Task<Result> ExecuteAsync(ConfirmPaymentCommand request, CancellationToken cancellationToken = default)
        {
            var payment = await paymentRepository.GetByAttemptIdAsync(request.AttemptId, cancellationToken);
            if (payment is null)
            {
                return Result.Failure(PaymentErrors.AttemptNotFound(request.AttemptId));
            }

            var result = await processor.ApplyAsync(
                payment,
                request.AttemptId,
                request.Outcome,
                request.ExternalReference,
                request.GatewayResultCode,
                request.GatewayResultMessage,
                cancellationToken);

            if (result.IsFailure)
            {
                return result;
            }

            return Result.Success();
        }
    }
}