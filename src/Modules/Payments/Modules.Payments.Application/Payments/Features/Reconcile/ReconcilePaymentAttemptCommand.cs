using FlashSales.Application.Messaging;

namespace Modules.Payments.Application.Payments.Features.Reconcile
{
    public sealed record ReconcilePaymentAttemptCommand(Guid AttemptId) : ICommand;
}
