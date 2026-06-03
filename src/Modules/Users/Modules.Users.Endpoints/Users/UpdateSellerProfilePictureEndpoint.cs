using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.Users.Features.UpdateProfilePicture;
using System.Security.Claims;

namespace Modules.Users.Endpoints.Users
{
    internal sealed class UpdateSellerProfilePictureEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch("api/v1/users/seller/picture", async (
                IFormFile file,
                ISender sender,
                ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken) =>
            {
                await using var stream = file.OpenReadStream();

                var result = await sender.SendAsync(new UpdateSellerProfilePictureCommand(
                    claimsPrincipal.GetUserId(),
                    stream,
                    file.ContentType
                    ), cancellationToken);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }).DisableAntiforgery()
              .WithTags(EndpointsModule.Module)
              .RequireAuthorization(UsersPermissions.Accounts.UpdateOwn)
              .WithSummary("Update the seller's profile picture")
              .WithDescription(
                  "Uploads a new profile picture for the currently authenticated seller. " +
                  "Accepted formats: JPEG, PNG, WebP. Maximum file size: 5 MB. " +
                  "The image is stored in blob storage and the URL is propagated to all modules that replicate seller data.")
              .Produces(StatusCodes.Status204NoContent)
              .ProducesProblem(StatusCodes.Status400BadRequest)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound);
        }
    }
}
