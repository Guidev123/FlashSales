using FlashSales.Application.Messaging;

namespace Modules.Users.Application.Users.UseCases.GetSeller
{
    public sealed record GetSellerQuery(Guid UserId) : IQuery<GetSellerResponse>;
}