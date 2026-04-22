using FluentValidation;
using Modules.Users.Domain.Users.Errors;

namespace Modules.Users.Application.Users.UseCases.ActivateSeller
{
    internal sealed class ActivateSellerCommandValidator : AbstractValidator<ActivateSellerCommand>
    {
        public ActivateSellerCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                    .WithErrorCode(SellerErrors.UserIdMustBeNotEmpty.Code)
                    .WithMessage(SellerErrors.UserIdMustBeNotEmpty.Description);

            RuleFor(x => x.Document)
                .NotEmpty()
                    .WithErrorCode(SellerErrors.DocumentMustBeNotEmpty.Code)
                    .WithMessage(SellerErrors.DocumentMustBeNotEmpty.Description);

            RuleFor(x => x.BankCode)
                .NotEmpty()
                    .WithErrorCode(SellerErrors.BankCodeMustBeNotEmpty.Code)
                    .WithMessage(SellerErrors.BankCodeMustBeNotEmpty.Description)
                .MaximumLength(3)
                    .WithErrorCode(SellerErrors.BankCodeTooLong.Code)
                    .WithMessage(SellerErrors.BankCodeTooLong.Description);

            RuleFor(x => x.Agency)
                .NotEmpty()
                    .WithErrorCode(SellerErrors.AgencyMustBeNotEmpty.Code)
                    .WithMessage(SellerErrors.AgencyMustBeNotEmpty.Description)
                .MaximumLength(10)
                    .WithErrorCode(SellerErrors.AgencyTooLong.Code)
                    .WithMessage(SellerErrors.AgencyTooLong.Description);

            RuleFor(x => x.AccountNumber)
                .NotEmpty()
                    .WithErrorCode(SellerErrors.AccountNumberMustBeNotEmpty.Code)
                    .WithMessage(SellerErrors.AccountNumberMustBeNotEmpty.Description)
                .MaximumLength(20)
                    .WithErrorCode(SellerErrors.AccountNumberTooLong.Code)
                    .WithMessage(SellerErrors.AccountNumberTooLong.Description);
        }
    }
}