using FlashSales.Application.Messaging;
using FlashSales.Domain.Results;
using Modules.Users.Domain.Users.Enum;
using Modules.Users.Domain.Users.Errors;
using Modules.Users.Domain.Users.Repositories;
using Modules.Users.Domain.Users.ValueObjects;

namespace Modules.Users.Application.Users.Features.UpdateSellerPaymentAccount
{
    internal sealed class UpdateSellerPaymentAccountCommandHandler(
        IUserRepository userRepository
        ) : ICommandHandler<UpdateSellerPaymentAccountCommand>
    {
        public async Task<Result> ExecuteAsync(UpdateSellerPaymentAccountCommand request, CancellationToken cancellationToken = default)
        {
            var seller = await userRepository.GetSellerAsync(request.UserId, cancellationToken);
            if (seller is null)
            {
                return Result.Failure(UserErrors.IsNotSeller(request.UserId));
            }

            if (!Enum.TryParse<BankAccountType>(request.AccountType, true, out var accountType))
            {
                return Result.Failure(UserErrors.FailedToParseAccountType);
            }

            seller.UpdatePaymentAccount(PaymentAccount.Create(
                request.BankCode,
                request.Agency,
                request.AccountNumber,
                accountType));

            userRepository.UpdateSeller(seller);

            return Result.Success();
        }
    }
}
