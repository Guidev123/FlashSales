using FlashSales.Application.Abstractions;
using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Launches.Contracts;
using Modules.Orders.Application.Abstractions;
using Modules.Orders.Application.Orders.Sagas;
using Modules.Orders.Domain.Orders.Repositories;
using Modules.Payments.Contracts;

namespace Modules.Orders.Application.Orders.Features.CompensateStaleSaga
{
    internal sealed class CompensateStaleOrderCreationSagaCommandHandler(
        IOrderRepository orderRepository,
        IOrderCreationSagaRepository sagaRepository,
        IOrdersUnitOfWork unitOfWork,
        ILaunchesPublicApi launchesPublicApi,
        IPaymentsPublicApi paymentsPublicApi
        ) : ICommandHandler<CompensateStaleOrderCreationSagaCommand>
    {
        private readonly OrderCreationSagaOrchestrator _orchestrator = new(
            orderRepository, sagaRepository, unitOfWork, launchesPublicApi, paymentsPublicApi);

        public Task<Result> ExecuteAsync(CompensateStaleOrderCreationSagaCommand request, CancellationToken cancellationToken = default)
        {
            return _orchestrator.CompensateAsync(
                request.SagaId,
                "Order creation saga stalled and was compensated by the sweep job",
                cancellationToken);
        }
    }
}
