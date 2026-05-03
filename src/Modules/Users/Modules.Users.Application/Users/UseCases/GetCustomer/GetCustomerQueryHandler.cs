using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Application.Users.Dtos;
using Modules.Users.Application.Users.Services;
using Modules.Users.Domain.Users.Errors;

namespace Modules.Users.Application.Users.UseCases.GetCustomer
{
    internal sealed class GetCustomerQueryHandler(IUserQueryService userQueryService) : IQueryHandler<GetCustomerQuery, UserResponse>
    {
        public async Task<Result<UserResponse>> ExecuteAsync(GetCustomerQuery request, CancellationToken cancellationToken = default)
        {
            var user = await userQueryService.GetAsync(request.UserId, cancellationToken);
            if (user is null)
            {
                return Result.Failure<UserResponse>(UserErrors.NotFound(request.UserId));
            }

            return user;
        }
    }
}