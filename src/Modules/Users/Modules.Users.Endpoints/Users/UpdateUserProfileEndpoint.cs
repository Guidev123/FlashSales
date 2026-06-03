using FlashSales.Domain.Results;
using FlashSales.Endpoints.Endpoints;
using FlashSales.Endpoints.Results;
using FlashSales.Infrastructure.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using MidR.Interfaces;
using Modules.Users.Application.Users.Features.UpdateUserProfile;
using System.Security.Claims;

namespace Modules.Users.Endpoints.Users
{
    internal sealed class UpdateUserProfileEndpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPatch("api/v1/users/profile", async (
                Request request,
                ISender sender,
                ClaimsPrincipal claimsPrincipal,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.SendAsync(new UpdateUserProfileCommand(
                    claimsPrincipal.GetUserId(),
                    claimsPrincipal.GetIdentityId(),
                    request.Name,
                    request.BirthDate
                    ), cancellationToken);

                return result.Match(Results.NoContent, ApiResults.Problem);
            }).WithTags(EndpointsModule.Module)
              .RequireAuthorization(UsersPermissions.Accounts.UpdateOwn)
              .WithSummary("Update the authenticated user's profile")
              .WithDescription(
                  "Updates the full name and date of birth of the currently authenticated user. " +
                  "Name must include at least a first and last name separated by a space. " +
                  "The user must be at least 16 years old. Changes are also propagated to the identity provider.")
              .Produces(StatusCodes.Status204NoContent)
              .ProducesProblem(StatusCodes.Status400BadRequest)
              .ProducesProblem(StatusCodes.Status401Unauthorized)
              .ProducesProblem(StatusCodes.Status403Forbidden)
              .ProducesProblem(StatusCodes.Status404NotFound);
        }

        record Request(string Name, DateTimeOffset BirthDate);
    }
}
