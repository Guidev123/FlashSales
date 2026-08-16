using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Payments.Application.Payments.Services;
using Modules.Payments.Domain.Payments.Errors;
using Modules.Payments.Domain.Payments.Repositories;

namespace Modules.Payments.Application.Payments.Features.Reconcile
{
    internal sealed class ReconcilePaymentAttemptCommandHandler(
        IPaymentRepository paymentRepository,
        IPaymentGatewayService paymentGatewayService,
        PaymentOutcomeProcessor processor
        ) : ICommandHandler<ReconcilePaymentAttemptCommand>
    {
        public async Task<Result> ExecuteAsync(ReconcilePaymentAttemptCommand request, CancellationToken cancellationToken = default)
        {
            var payment = await paymentRepository.GetByAttemptIdAsync(request.AttemptId, cancellationToken);
            if (payment is null)
            {
                return Result.Failure(PaymentErrors.AttemptNotFound(request.AttemptId));
            }

            var attempt = payment.Attempts.FirstOrDefault(a => a.Id == request.AttemptId);
            if (attempt?.GatewaySessionId is null)
            {
                return Result.Failure(PaymentErrors.AttemptNotFound(request.AttemptId));
            }

            var checkResult = await paymentGatewayService.CheckAttemptStatusAsync(attempt.GatewaySessionId, cancellationToken);
            if (checkResult.IsFailure)
            {
                return checkResult;
            }

            var result = await processor.ApplyAsync(
                payment,
                request.AttemptId,
                checkResult.Value.Outcome,
                checkResult.Value.ExternalReference,
                checkResult.Value.GatewayResultCode,
                checkResult.Value.GatewayResultMessage,
                cancellationToken);

            if (result.IsFailure)
            {
                return result;
            }

            return Result.Success();
        }
    }
}