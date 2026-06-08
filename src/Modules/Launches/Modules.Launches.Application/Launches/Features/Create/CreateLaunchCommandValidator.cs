using FluentValidation;
using Modules.Launches.Domain.Launches.Errors;
using Modules.Launches.Domain.Launches.ValueObjects;

namespace Modules.Launches.Application.Launches.Features.Create
{
    internal sealed class CreateLaunchCommandValidator : AbstractValidator<CreateLaunchCommand>
    {
        public CreateLaunchCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEqual(Guid.Empty)
                .WithMessage(LaunchErrors.SellerIdRequired.Description);

            RuleFor(x => x.ProductId)
                .NotEqual(Guid.Empty)
                .WithMessage(LaunchErrors.ProductIdRequired.Description);

            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage(LaunchErrors.TitleRequired.Description)
                .MaximumLength(LaunchMetadata.TITLE_MAX_LENGTH)
                .WithMessage($"Title must not exceed {LaunchMetadata.TITLE_MAX_LENGTH} characters");

            RuleFor(x => x.Description)
                .NotEmpty()
                .WithMessage("Description must not be empty")
                .MaximumLength(LaunchMetadata.DESCRIPTION_MAX_LENGTH)
                .WithMessage($"Description must not exceed {LaunchMetadata.DESCRIPTION_MAX_LENGTH} characters");
        }
    }
}
