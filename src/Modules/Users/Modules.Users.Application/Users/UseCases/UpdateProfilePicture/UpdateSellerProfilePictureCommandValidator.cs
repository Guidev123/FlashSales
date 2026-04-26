using FluentValidation;
using Modules.Users.Domain.Users.Errors;

namespace Modules.Users.Application.Users.UseCases.UpdateProfilePicture
{
    internal sealed class UpdateSellerProfilePictureCommandValidator : AbstractValidator<UpdateSellerProfilePictureCommand>
    {
        private const long MaxFileSizeInBytes = 5 * 1024 * 1024;

        private static readonly string[] AllowedContentTypes =
            ["image/jpeg", "image/png", "image/webp"];

        public UpdateSellerProfilePictureCommandValidator()
        {
            RuleFor(x => x.File)
                .Must(stream => stream.Length > 0)
                .WithMessage(UserErrors.ProfilePictureIsEmpty.Description)
                .Must(stream => stream.Length <= MaxFileSizeInBytes)
                .WithMessage(UserErrors.ProfilePictureTooLarge.Description);

            RuleFor(x => x.ContentType)
                .Must(ct => AllowedContentTypes.Contains(ct))
                .WithMessage(UserErrors.ProfilePictureInvalidContentType.Description);
        }
    }
}