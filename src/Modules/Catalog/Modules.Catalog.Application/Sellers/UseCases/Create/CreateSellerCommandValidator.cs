using FluentValidation;
using Modules.Catalog.Domain.Sellers.Errors;

namespace Modules.Catalog.Application.Sellers.UseCases.Create
{
    internal sealed class CreateSellerCommandValidator : AbstractValidator<CreateSellerCommand>
    {
        public CreateSellerCommandValidator()
        {
            RuleFor(u => u.UserId)
                .NotEqual(Guid.Empty)
                .WithMessage(SellerErrors.UserIdRequired.Description);

            RuleFor(u => u.SellerId)
                .NotEqual(Guid.Empty)
                .WithMessage(SellerErrors.SellerIdRequired.Description);

            RuleFor(u => u.Name)
                .NotEmpty()
                .WithMessage(SellerErrors.NameMustNotBeEmpty.Description);
        }
    }
}