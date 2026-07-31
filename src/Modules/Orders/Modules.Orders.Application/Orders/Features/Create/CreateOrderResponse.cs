namespace Modules.Orders.Application.Orders.Features.Create
{
    public sealed record CreateOrderResponse(Guid OrderId, string OrderCode, string CheckoutUrl);
}
