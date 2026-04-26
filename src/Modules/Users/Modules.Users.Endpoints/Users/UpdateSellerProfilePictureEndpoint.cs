using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.Users.UseCases.UpdateProfilePicture;
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
              .WithDescription("Update seller profile picture");
        }
    }
}