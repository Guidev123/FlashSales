using FluentValidation;

namespace Modules.Users.Application.Users.UseCases.ActivateSeller
{
    internal sealed class ActivateSellerCommandValidator : AbstractValidator<ActivateSellerCommand>
    {
        public ActivateSellerCommandValidator()
        {
        }
    }
}