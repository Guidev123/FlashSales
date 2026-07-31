namespace Modules.Orders.Application.Orders.Dtos
{
    public sealed record OrderResponse(
        Guid Id,
        Guid LaunchId,
        string? LaunchTitle,
        Guid ProductId,
        Guid SellerId,
        int Quantity,
        decimal UnitPrice,
        decimal TotalAmount,
        string OrderCode,
        string Status,
        DateTimeOffset ExpiresAt,
        DateTimeOffset? ConfirmedAt,
        string? Reason,
        DateTimeOffset CreatedOn
        );
}
