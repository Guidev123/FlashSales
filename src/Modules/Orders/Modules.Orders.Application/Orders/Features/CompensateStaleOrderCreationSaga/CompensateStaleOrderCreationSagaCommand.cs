using FlashSales.Application.Messaging;

namespace Modules.Orders.Application.Orders.Features.CompensateStaleSaga
{
    public sealed record CompensateStaleOrderCreationSagaCommand(Guid SagaId) : ICommand;
}
