using FlashSales.Application.Messaging;
using Modules.Users.Application.Users.Dtos;

namespace Modules.Users.Application.Users.Features.GetCustomer
{
    public sealed record GetCustomerQuery(Guid UserId) : IQuery<UserResponse>;
}