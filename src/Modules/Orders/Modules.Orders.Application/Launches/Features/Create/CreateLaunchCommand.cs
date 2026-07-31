using FlashSales.Application.Messaging;

namespace Modules.Orders.Application.Launches.Features.Create
{
    public sealed record CreateLaunchCommand(
        Guid LaunchId,
        Guid SellerId,
        Guid ProductId,
        string Title,
        decimal DiscountedPrice,
        decimal OriginalPrice,
        int TotalQuantity,
        DateTimeOffset StartAt,
        DateTimeOffset EndAt
        ) : ICommand;
}
