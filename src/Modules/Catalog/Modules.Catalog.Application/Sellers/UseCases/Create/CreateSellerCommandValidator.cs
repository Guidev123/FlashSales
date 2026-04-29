using FluentValidation;

namespace Modules.Catalog.Application.Sellers.UseCases.Create
{
    internal sealed class CreateSellerCommandValidator : AbstractValidator<CreateSellerCommand>
    {
        public CreateSellerCommandValidator()
        {
        }
    }
}