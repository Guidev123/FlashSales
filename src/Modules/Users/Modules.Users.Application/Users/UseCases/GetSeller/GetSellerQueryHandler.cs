using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Application.Users.Repositories;
using Modules.Users.Domain.Users.Errors;

namespace Modules.Users.Application.Users.UseCases.GetSeller
{
    internal sealed class GetSellerQueryHandler(IUserRepository userRepository) : IQueryHandler<GetSellerQuery, GetSellerResponse>
    {
        public async Task<Result<GetSellerResponse>> ExecuteAsync(GetSellerQuery request, CancellationToken cancellationToken = default)
        {
            var result = await userRepository.GetSellerProfileAsync(request.UserId, cancellationToken);
            if (result is null)
            {
                return Result.Failure<GetSellerResponse>(UserErrors.SellerNotFound(request.UserId));
            }

            return result;
        }
    }
}