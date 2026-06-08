using FlashSales.Application.Messaging;

namespace Modules.Launches.Application.Launches.Features.Schedule
{
    public sealed record ScheduleLaunchCommand(
        Guid UserId,
        Guid LaunchId,
        decimal DiscountedPrice,
        decimal OriginalPrice,
        int TotalQuantity,
        int ReservedQuantity,
        DateTimeOffset StartAt,
        DateTimeOffset EndAt
        ) : ICommand;
}