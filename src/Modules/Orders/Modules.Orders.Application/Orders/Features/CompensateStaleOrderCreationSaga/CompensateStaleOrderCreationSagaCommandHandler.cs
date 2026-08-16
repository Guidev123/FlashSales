using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Orders.Application.Orders.Features.CompensateStaleSaga;
using Modules.Orders.Application.Orders.Sagas;

namespace Modules.Orders.Application.Orders.Features.CompensateStaleOrderCreationSaga
{
    internal sealed class CompensateStaleOrderCreationSagaCommandHandler(
        OrderCreationSagaOrchestrator orchestrator
        ) : ICommandHandler<CompensateStaleOrderCreationSagaCommand>
    {
        public Task<Result> ExecuteAsync(CompensateStaleOrderCreationSagaCommand request, CancellationToken cancellationToken = default)
        {
            return orchestrator.CompensateAsync(
                request.SagaId,
                "Order creation saga stalled and was compensated by the sweep job",
                cancellationToken);
        }
    }
}