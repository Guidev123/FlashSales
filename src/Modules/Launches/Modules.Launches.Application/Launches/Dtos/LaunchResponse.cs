namespace Modules.Launches.Application.Launches.Dtos
{
    public sealed record LaunchResponse(
        Guid Id,
        Guid SellerId,
        Guid ProductId,
        string Title,
        string Description,
        decimal? DiscountedPrice,
        decimal? OriginalPrice,
        decimal? DiscountPercentage,
        int? TotalQuantity,
        int? ReservedQuantity,
        int? AvailableQuantity,
        DateTimeOffset? StartAt,
        DateTimeOffset? EndAt,
        string Status,
        DateTimeOffset CreatedOn
    );
}
