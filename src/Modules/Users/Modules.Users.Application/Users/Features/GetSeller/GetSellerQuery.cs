using FlashSales.Application.Messaging;

namespace Modules.Users.Application.Users.Features.GetSeller
{
    public sealed record GetSellerQuery(Guid UserId) : IQuery<GetSellerResponse>;
}